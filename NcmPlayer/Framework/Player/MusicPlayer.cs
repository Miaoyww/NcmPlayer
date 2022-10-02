﻿using NAudio.Wave;
using NcmPlayer.Resources;
using NcmPlayer.Views;
using System;
using System.ComponentModel;
using System.Timers;

namespace NcmPlayer.Framework.Player
{
    public class MusicPlayer: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        private WaveOutEvent outputDevice;
        private MediaFoundationReader mfr;
        private Timer updateInfo = new();

        private bool isPlaying;

        public bool IsPlaying
        {
        }

        public void InitPlayer()
        {
            outputDevice = new();
            updateInfo.Elapsed += Timer_Elapsed;
            updateInfo.Interval = 100;
            updateInfo.Start();
        }

        public void Reload()
        {

        }

        // 信息更新
        public void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            MainWindow.acc.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (outputDevice != null)
                {
                    if (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        TimeSpan convered = TimeSpan.FromSeconds(mfr.Position / outputDevice.OutputWaveFormat.BitsPerSample / outputDevice.OutputWaveFormat.Channels * 8.0 / outputDevice.OutputWaveFormat.SampleRate);
                        ResEntry.musicInfo.Postion = convered;
                    }
                }
            }));
        }

        public void RePlay(string path, string name, string artists)
        {
            ResEntry.musicInfo.Name = name;
            ResEntry.musicInfo.Artists = artists;
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
            }
            if (mfr == null)
            {
                mfr = new MediaFoundationReader(path);
                outputDevice.Init(mfr);
                outputDevice.Volume = (float)ResEntry.musicInfo.Volume / 100;
            }
            else
            {
                if (path != null)
                {
                    outputDevice.Stop();
                    mfr = new MediaFoundationReader(path);
                    outputDevice.Init(mfr);
                    outputDevice.Volume = (float)ResEntry.musicInfo.Volume / 100;
                }
            }
            Play(true, path);
        }

        public void Play(bool re = false, string url = null)
        {
            try
            {
                if (re)
                {
                    ResEntry.musicInfo.IsPlaying = true;
                    mfr.Position = TimeSpan.Zero.Ticks;
                    outputDevice.Play();
                }
                else
                {
                    if (!ResEntry.musicInfo.IsPlaying)
                    {
                        outputDevice.Play();
                        ResEntry.musicInfo.IsPlaying = true;
                        // ResEntry.musicInfo.DurationTime = outputDevice.GetPositionTimeSpan
                    }
                    else
                    {
                        outputDevice.Pause();
                        ResEntry.musicInfo.IsPlaying = false;
                        // ResEntry.musicInfo.DurationTime = player.NaturalDuration.TimeSpan;
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Volume(double volume)
        {
            if (outputDevice != null)
            {
                outputDevice.Volume = (float)volume;
            }
        }

        public void Position(double position)
        {
            if (ResEntry.musicInfo.IsPlaying)
            {
                long pos = (long)(position * 10) * outputDevice.OutputWaveFormat.BitsPerSample * outputDevice.OutputWaveFormat.Channels * outputDevice.OutputWaveFormat.SampleRate / 8 / 10;
                mfr.Position = pos;
                TimeSpan convered = TimeSpan.FromSeconds(mfr.Position / outputDevice.OutputWaveFormat.BitsPerSample / outputDevice.OutputWaveFormat.Channels * 8.0 / outputDevice.OutputWaveFormat.SampleRate);
                ResEntry.musicInfo.Postion = convered;
            }
        }
    }
}