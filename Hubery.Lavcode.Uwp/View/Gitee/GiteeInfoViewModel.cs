﻿using GalaSoft.MvvmLight;
using Hubery.Lavcode.Uwp.Model.Api;
using Hubery.Tools;
using Hubery.Tools.Uwp.Helpers;
using Hubery.Yt.Uwp.Helpers;
using System;
using System.Threading.Tasks;

namespace Hubery.Lavcode.Uwp.View.Gitee
{
    public class GiteeInfoViewModel : ViewModelBase
    {
        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { Set(ref _loading, value); }
        }

        private Repository _repository = null;
        public Repository Repository
        {
            get { return _repository; }
            set { Set(ref _repository, value); }
        }

        public async Task Init()
        {
            if (Repository != null)
            {
                return;
            }

            Loading = true;
            await TaskExtend.SleepAsync(100);

            try
            {
                Repository = await ApiExtendHelper.GetRepos();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex, 0);
                return;
            }
            finally
            {
                Loading = false;
            }
        }
    }
}