﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenRM;
using OpenRM.Convert;
using theori.Audio;
using theori.Graphics;
using theori.Gui;

namespace theori.Game.Scenes
{
    /// <summary>
    /// The main scene for the entire editor.
    /// A huge "this is everything" core class is really not a great idea, but
    ///  there's little else to do in a program which only does one thing.
    ///  
    /// The idea is that this will drive most of the important editing logic but not
    ///  specific input handling and rendering.
    /// Shared data and state information will be stored and manipulated here, and the
    ///  different views will be updated with them to keep them synchronized.
    /// User input data should come mostly from those sub-areas as well, so the 2D
    ///  and 3D views propogate input events upwards.
    /// </summary>
    public sealed class EditorCore : Scene
    {
        enum State
        {
            NoChartLoaded,
            ChartIdle,
            ChartPlaying,
        }

        enum Tool
        {
            // AnyBt and AnyFx will be separate options that shadow the following two tools
            //  if enabled; it will allow editing of both chip and hold with a single tool.
            AnyBt, BtChip, BtHold,
            AnyFx, FxChip, FxHold,
            VolL, VolR,
        }

        #region Actual Chart Data

        private Chart m_chart;
        private AudioTrack m_rawAudio;

        #endregion

        #region Gui

        private Panel m_uiRoot;
        private GuiManager m_guiManager;

        #endregion

        #region Playback
        
        private tick_t m_cursorPos;

        // TODO(local): I'm not sure if the 2d view will need the same kind of "playback"
        //  since it's a series of static views.
        // The only thing that might have been necessary is audio events, but the audio
        //  track will be pre-rendered, so any ole playback will work.
        // This means that the current plan is likely to use this sliding playback
        //  to process the audio-related events (and probably also bake in slams) so that audio
        //  can be played without regard to effects even existing.
        // Editing objects which mess with effects will obviously have to re-render the audio,
        //  and that hopefully can be done locally.

        private SlidingChartPlayback m_playback3D;

        #endregion

        #region Editor View State

        enum ViewKind
        {
            /// <summary>
            /// Show objects and data compatible with that view.
            /// This will be BT/FX/VOL, timing info, stops/rewinds/spins, things like that.
            /// </summary>
            Objects,
            /// <summary>
            /// Focuses on the camera controls and leaves the objects dimmed in the background.
            /// </summary>
            LinearParams,
        }

        /// <summary>
        /// What to show in the different views.
        /// </summary>
        private ViewKind m_viewKind = ViewKind.Objects;

        #endregion

        #region Construction

        private string m_initialChartToLoad;

        public EditorCore(string chartToLoad = null)
        {
            // if null, means nothing anyway
            m_initialChartToLoad = chartToLoad;
        }

        #endregion

        #region Chart Load/Save

        private void LoadChart(string chartPath)
        {
            if (Path.GetExtension(chartPath) == ".ksh")
            {
                string fileDir = Directory.GetParent(chartPath).FullName;
                var ksh = KShootMania.Chart.CreateFromFile(chartPath);
                    
                string audioFileFx = Path.Combine(fileDir, ksh.Metadata.MusicFile ?? "");
                string audioFileNoFx = Path.Combine(fileDir, ksh.Metadata.MusicFileNoFx ?? "");

                string audioFile = audioFileNoFx;
                if (File.Exists(audioFileFx))
                    audioFile = audioFileFx;

                if (!File.Exists(audioFile))
                {
                    Logger.Log($"Couldn't find audio file for chart at path \"{ audioFile }\"");
                    return;
                }

                m_rawAudio = AudioTrack.FromFile(audioFile);
                // TODO(local): stick audio in a separate channel probably
                m_rawAudio.Channel = Host.Mixer.MasterChannel;
                m_rawAudio.Volume = ksh.Metadata.MusicVolume / 100.0f;

                m_chart = ksh.ToVoltex();
            }
            else
            {
                Logger.Log($"Unrecognized chart file \"{ chartPath }\"");
            }
        }

        #endregion

        public override void Init()
        {
            Logger.Log($">> Loading Editor for the first time:");

            m_uiRoot = new Panel();
            m_guiManager = new GuiManager(m_uiRoot);

            // after init, load a chart if specified.
            if (m_initialChartToLoad != null)
            {
                Logger.Log($"Loading initial chart from \"{ m_initialChartToLoad }\"");

                LoadChart(m_initialChartToLoad);
                m_initialChartToLoad = null;
            }
        }

        public override void Update()
        {
            #region Resize the root panel after everything else is done I guess

            m_uiRoot.Position = Vector2.Zero;
            m_uiRoot.RelativeSizeAxes = Axes.None;
            m_uiRoot.Size = new Vector2(Window.Width, Window.Height);
            m_uiRoot.Rotation = 0;
            m_uiRoot.Scale = Vector2.One;
            m_uiRoot.Origin = Vector2.Zero;

            #endregion
        }

        public override void Render()
        {
            if (m_uiRoot != null)
            {
                using (var grq = new GuiRenderQueue(m_uiRoot.Size))
                    m_uiRoot.Render(grq);
            }
        }
    }
}
