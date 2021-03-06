﻿using GalaSoft.MvvmLight;
using Lavcode.Uwp.View.Main;
using HTools.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Lavcode.Uwp.View.Auth
{
    public class AuthViewModel : ViewModelBase
    {
        private bool _isSupportLogin = true;
        public bool IsSupportLogin
        {
            get { return _isSupportLogin; }
            set { Set(ref _isSupportLogin, value); }
        }

        private bool _isLogin = false;
        public bool IsLogin
        {
            get { return _isLogin; }
            set { Set(ref _isLogin, value); }
        }

        public async Task Init()
        {
            try
            {
                await Login();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex);
            }
            finally
            {
                LoadingHelper.Hide();
            }
        }

        public async void LoginCommand()
        {
            try
            {
                await Login();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex);
            }
        }

        private async Task Login()
        {
            if (IsLogin)
            {
                return;
            }

            IsLogin = true;
            try
            {
                if (await KeyCredentialManager.IsSupportedAsync())
                {
                    KeyCredentialRetrievalResult keyCreationResult = await KeyCredentialManager.RequestCreateAsync(SystemInformation.ApplicationName, KeyCredentialCreationOption.ReplaceExisting);
                    if (keyCreationResult.Status != KeyCredentialStatus.Success)
                    {
                        return;
                    }
                    else
                    {
                        (Window.Current.Content as Frame)?.Navigate(typeof(MainPage));
                    }
                }
                else
                {
                    MessageHelper.ShowDanger("请在系统设置中打开Windows Hello");
                    IsSupportLogin = false;
                }
            }
            finally
            {
                IsLogin = false;
            }
        }
    }
}