﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Lavcode.Uwp.Helpers;
using Lavcode.Uwp.Helpers.Sqlite;
using Lavcode.Uwp.Model;
using HTools.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Lavcode.Uwp.View.FolderList
{
    public class FolderListViewModel : ViewModelBase
    {
        public ObservableCollection<FolderItem> FolderItems { get; } = new ObservableCollection<FolderItem>();

        private bool _initing = false;
        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_initing || value < 0 || _selectedIndex == value)
                {
                    return;
                }

                Set(ref _selectedIndex, value);
                SettingHelper.Instance.SelectedFolderId = value < FolderItems.Count ? FolderItems[value].Folder.Id : null;
                Messenger.Default.Send(value < FolderItems.Count ? FolderItems[value] : null, "FolderSelected");
            }
        }

        #region Init
        public FolderListViewModel()
        {
            Messenger.Default.Register<object>(this, "OnDbRecovered", async (obj) => await OnDbRecovered());
        }

        ~FolderListViewModel()
        {
            Messenger.Default.Unregister(this);
        }

        private async Task OnDbRecovered()
        {
            await Refresh();
        }

        public async Task Refresh()
        {
            try
            {
                _initing = true;
                FolderItems.Clear();
                await foreach (var folder in FolderHelper.GetFolderItems())
                {
                    FolderItems.Add(folder);
                }
                _initing = false;

                var beforeIndex = SelectedIndex;
                var selectedFolderItem = FolderItems.Where(item => item.Folder.Id == SettingHelper.Instance.SelectedFolderId).FirstOrDefault();
                if (selectedFolderItem == default)
                {
                    SelectedIndex = 0;
                }
                else
                {
                    SelectedIndex = FolderItems.IndexOf(selectedFolderItem);
                }
                if (SelectedIndex == beforeIndex) // 选中的Index没有变化
                {
                    RaisePropertyChanged(nameof(SelectedIndex));
                    Messenger.Default.Send(SelectedIndex < FolderItems.Count ? FolderItems[SelectedIndex] : null, "FolderSelected");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex);
            }
        }
        #endregion

        public async void HandleAddFolder()
        {
            var editFolder = new EditFolder();
            if (await editFolder.ShowAsync() == ContentDialogResult.Primary)
            {
                var newFolderItem = new FolderItem(editFolder.Folder, editFolder.Icon);
                FolderItems.Add(newFolderItem);
                var index = FolderItems.IndexOf(newFolderItem);

                if (SelectedIndex != index)
                {
                    SelectedIndex = index;
                }
                else
                {
                    Messenger.Default.Send(newFolderItem, "FolderSelected");
                }
            }
        }

        public async Task DeleteFolder(FolderItem folderItem)
        {
            if (await PopupHelper.ShowDialog($"确认删除{folderItem.Name}及其中所有密码项？该操作不可恢复！", "确认删除", "确定", "取消", false) != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                if (folderItem == null || !FolderItems.Contains(folderItem)) //正常情况不满足
                {
                    return;
                }

                using var helper = new SqliteHelper();
                await helper.DeleteFolder(folderItem.Folder.Id);
                FolderItems.Remove(folderItem);

                if (FolderItems.Count == 0) // 删完了
                {
                    Messenger.Default.Send<FolderItem>(null, "FolderSelected");
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex);
            }
        }

        public async Task EditFolder(FolderItem folderItem)
        {
            var editFolder = new EditFolder(folderItem.Folder);
            if (await editFolder.ShowAsync() == ContentDialogResult.Primary)
            {
                folderItem.Set(editFolder.Folder, editFolder.Icon);
            }
        }

        #region 排序
        public async void DragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        {
            try
            {
                if (args.DropResult == Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move)
                {
                    await Sort();
                }
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError(ex);
            }

        }

        public async Task Sort()
        {
            using var helper = new SqliteHelper();
            for (var i = 0; i < FolderItems.Count; i++)
            {
                FolderItems[i].Folder.Order = i;
                await helper.UpdateFolder(FolderItems[i].Folder);
            }
        }
        #endregion
    }
}