﻿using NcmApi;
using NcmPlayer.CloudMusic;
using NcmPlayer.Resources;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NcmPlayer.Views.Pages.LoginPage
{
    /// <summary>
    /// Loggedin.xaml 的交互逻辑
    /// </summary>
    public partial class Loggedin : Page
    {
        public Loggedin()
        {
            InitializeComponent();
            DataContext = ResEntry.res;
            tblock_name.Text = Login.acc.Name;

            if (string.IsNullOrEmpty(Login.acc.Id))
            {
                RegetAccountDetail();
            }
            if (ResEntry.res.allowDailySignin)
            {
                if (int.Parse(DateTime.Now.ToString("yyyyMMdd")) - Login.acc.LastSignin >= 1)
                {
                    JObject result = Api.User.DailyTask("1", ResEntry.ncm);
                    if (result["code"].ToString() == "-2")
                    {
                        PublicMethod.SnackLog("今天已经完成签到啦", "注意", 2000);
                        Regediter.Regedit("Account", "LastSignin", DateTime.Now.ToString("yyyyMMdd"));
                    }
                    else if (result["code"].ToString() == "200")
                    {
                        PublicMethod.SnackLog("签到成功", "注意", 2000);
                        Regediter.Regedit("Account", "LastSignin", DateTime.Now.ToString("yyyyMMdd"));
                    }
                    else if (result["code"].ToString() == "301")
                    {
                        PublicMethod.SnackLog("签到失败: 未登录", "注意", 2000);
                    }
                }
            }
            try
            {
                Face.Background = PublicMethod.ConvertBrush(new MemoryStream(Convert.FromBase64String(RegGeter.RegGet("Account", "AccountFace").ToString())));
                MainWindow.acc.b_user.Child = null;
                MainWindow.acc.b_user.BorderThickness = new Thickness(0);
                MainWindow.acc.b_user.Background = PublicMethod.ConvertBrush(new MemoryStream(Convert.FromBase64String(RegGeter.RegGet("Account", "AccountFace").ToString())));
            }
            catch
            {
            }
        }

        public void RegetAccountDetail()
        {
            JObject accountDetail = Api.User.Account(ResEntry.ncm);
            Login.acc.Id = accountDetail["account"]["id"].ToString();
            Login.acc.Name = accountDetail["profile"]["nickname"].ToString();
            Login.acc.FaceUrl = accountDetail["profile"]["avatarUrl"].ToString();
            Login.acc.FaceStream = HttpRequest.StreamHttpGet(Login.acc.FaceUrl);
            MemoryStream ms = new();
            Login.acc.FaceStream.CopyTo(ms);
            Regediter.Regedit("Account", "AccountFace", Convert.ToBase64String(ms.ToArray()));

            Face.Background = PublicMethod.ConvertBrush(new MemoryStream(Convert.FromBase64String(RegGeter.RegGet("Account", "AccountFace").ToString())));
            tblock_name.Text = Login.acc.Name;

            Regediter.Regedit("Account", "AccountFaceUrl", Login.acc.FaceUrl);
            Regediter.Regedit("Account", "AccountName", Login.acc.Name);
            Regediter.Regedit("Account", "AccountId", Login.acc.Id);
        }
    }
}