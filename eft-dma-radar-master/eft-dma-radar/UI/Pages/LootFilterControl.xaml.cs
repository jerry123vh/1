﻿using eft_dma_radar.Tarkov.Loot;
using eft_dma_radar.UI.LootFilters;
using eft_dma_radar.UI.Misc;
using eft_dma_shared.Common.Misc;
using eft_dma_shared.Common.Misc.Data;
using HandyControl.Controls;
using HandyControl.Tools;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using MessageBox = eft_dma_shared.Common.UI.Controls.MessageBox;

namespace eft_dma_radar.UI.Pages
{
    public partial class LootFilterControl : UserControl, INotifyPropertyChanged
    {
        #region Fields and Properties
        public event PropertyChangedEventHandler PropertyChanged;

        public static string SearchString;
        public static bool ShowMeds;
        public static bool ShowFood;
        public static bool ShowBackpacks;
        public static bool firstRemove = false;
        private static Config Config => Program.Config;
        private static bool ShowQuestItems => Config.QuestHelper.Enabled;

        private ObservableCollection<LootFilterGroup> _groupList = new();
        private LootFilterGroup _selectedGroup;
        private ObservableCollection<GroupedLootFilterEntry> _groupItems = new();
        private ICollectionView _groupItemsView;

        private Point _dragStartPoint;

        private const int INTERVAL = 100; // 0.1 second
        private const string DEFAULT_GROUP_NAME = "default";

        private bool _isUpdatingControls = false;

        public ObservableCollection<LootFilterGroup> GroupList => _groupList;

        public event EventHandler CloseRequested;
        public event EventHandler BringToFrontRequested;
        public event EventHandler<PanelDragEventArgs> DragRequested;
        public event EventHandler<PanelResizeEventArgs> ResizeRequested;

        private HandyControl.Controls.PopupWindow _openColorPicker;
        #endregion

        public LootFilterControl()
        {
            InitializeComponent();
            TooltipManager.AssignLootFilterTooltips(this);

            this.Loaded += async (s, e) =>
            {
                while (MainWindow.Config == null)
                {
                    await Task.Delay(INTERVAL);
                }

                PanelCoordinator.Instance.SetPanelReady("LootFilter");
                ExpanderManager.Instance.RegisterExpanders(this, "LootFilter",
                    expLootFilterSettings,
                    expLootFilterItems);

                try
                {
                    await PanelCoordinator.Instance.WaitForAllPanelsAsync();

                    InitializeControlEvents();
                    LoadSettings();
                }
                catch (TimeoutException ex)
                {
                    LoneLogging.WriteLine($"[PANELS] {ex.Message}");
                }
            };
        }

        #region Filter Settings Panel
        #region Functions/Methods
        private void InitializeControlEvents()
        {
            Dispatcher.InvokeAsync(() =>
            {
                RegisterPanelEvents();
                RegisterSettingsEvents();
            });
        }

        private void RegisterPanelEvents()
        {
            // Header close button
            btnCloseHeader.Click += btnCloseHeader_Click;

            // Drag handling
            DragHandle.MouseLeftButtonDown += DragHandle_MouseLeftButtonDown;
        }
        #endregion

        #region Events
        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BringToFrontRequested?.Invoke(this, EventArgs.Empty);


            DragHandle.CaptureMouse();
            _dragStartPoint = e.GetPosition(this);

            DragHandle.MouseMove += DragHandle_MouseMove;
            DragHandle.MouseLeftButtonUp += DragHandle_MouseLeftButtonUp;
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _dragStartPoint;
                DragRequested?.Invoke(this, new PanelDragEventArgs(offset.X, offset.Y));
            }
        }

        private void DragHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DragHandle.ReleaseMouseCapture();
            DragHandle.MouseMove -= DragHandle_MouseMove;
            DragHandle.MouseLeftButtonUp -= DragHandle_MouseLeftButtonUp;
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).CaptureMouse();
            _dragStartPoint = e.GetPosition(this);
            ((UIElement)sender).MouseMove += ResizeHandle_MouseMove;
            ((UIElement)sender).MouseLeftButtonUp += ResizeHandle_MouseLeftButtonUp;
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var sizeDelta = currentPosition - _dragStartPoint;
                ResizeRequested?.Invoke(this, new PanelResizeEventArgs(sizeDelta.X, sizeDelta.Y));
                _dragStartPoint = currentPosition;
            }
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            ((UIElement)sender).MouseMove -= ResizeHandle_MouseMove;
            ((UIElement)sender).MouseLeftButtonUp -= ResizeHandle_MouseLeftButtonUp;
        }

        private void btnCloseHeader_Click(object sender, RoutedEventArgs e)
        {
            _openColorPicker?.Close();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_selectedGroup != null)
                _selectedGroup.PropertyChanged -= OnGroupChanged;

            foreach (var item in _groupItems)
                item.PropertyChanged -= OnItemChanged;
        }

        private void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
        #endregion

        #region Loot Filter Settings
        #region Functions/Methods
        private void RegisterSettingsEvents()
        {
            // Loot Filter Settings
            btnMenu.Click += LootFilterButton_Click;
            mnuExportSelected.Click += LootFilterMenuItem_Click;
            mnuExportAll.Click += LootFilterMenuItem_Click;
            mnuImport.Click += LootFilterMenuItem_Click;

            cboLootFilters.SelectionChanged += cboLootFilters_SelectionChanged;
            btnRemoveGroup.Click += LootFilterButton_Click;
            btnAddGroup.Click += LootFilterButton_Click;

            chkEnabled.Checked += LootFilterCheckbox_CheckChanged;
            chkEnabled.Unchecked += LootFilterCheckbox_CheckChanged;
            chkStatic.Checked += LootFilterCheckbox_CheckChanged;
            chkStatic.Unchecked += LootFilterCheckbox_CheckChanged;
            chkNotify.Checked += LootFilterCheckbox_CheckChanged;
            chkNotify.Unchecked += LootFilterCheckbox_CheckChanged;

            nudNotifyTime.ValueChanged += LootFilterNumericUpDown_ValueChanged;
            nudGroupIndex.ValueChanged += LootFilterNumericUpDown_ValueChanged;

            txtGroupName.TextChanged += LootFilterTextBox_TextChanged;

            // Loot Filter Items
            btnAddItem.Click += LootFilterButton_Click;
            btnRemoveItem.Click += LootFilterButton_Click;
            txtItemSearch.TextChanged += LootFilterTextBox_TextChanged;
        }

        private void LoadSettings()
        {
            LootFilterManager.Load();

            _groupList.Clear();

            foreach (var group in LootFilterManager.CurrentGroups.Groups.OrderBy(g => g.Index))
                _groupList.Add(group);

            if (_groupList.Count == 0)
                EnsureDefaultLootFilter();

            UpdateUIControls();

            if (_groupList.Count > 0)
            {
                cboLootFilters.ItemsSource = _groupList;
                cboLootFilters.SelectedIndex = 0;

                SetSelectedGroup(_groupList[0]);
            }

            if (cboItems.Items.Count < 1)
                SearchForItem();
        }

        /// <summary>
        /// Ensures at least one filter group exists.
        /// </summary>
        private void EnsureDefaultLootFilter()
        {
            if (_groupList.Count == 0)
            {
                LoneLogging.WriteLine("[Filters] Creating default filter group.");

                var defaultGroup = new LootFilterGroup
                {
                    Name = DEFAULT_GROUP_NAME,
                    Index = 0,
                    Enabled = true,
                    IsStatic = true,
                    Items = new List<GroupedLootFilterEntry>()
                };

                _groupList.Add(defaultGroup);

                LootFilterManager.CurrentGroups.Groups.Add(defaultGroup);
                LootFilterManager.Save();

                SetSelectedGroup(defaultGroup);

                NotificationsShared.Info("[Filters] Created default filter group.");
            }
        }

        private void SetSelectedGroup(LootFilterGroup group)
        {
            if (_selectedGroup != null)
            {
                _selectedGroup.PropertyChanged -= OnGroupChanged;

                foreach (var item in _groupItems)
                    item.PropertyChanged -= OnItemChanged;
            }

            _selectedGroup = group;

            if (_selectedGroup != null)
            {
                _selectedGroup.PropertyChanged += OnGroupChanged;
                _groupItems = new ObservableCollection<GroupedLootFilterEntry>(_selectedGroup.Items);

                foreach (var item in _groupItems)
                    item.PropertyChanged += OnItemChanged;

                GroupedItemsListView.ItemsSource = _groupItems;
                _groupItemsView = CollectionViewSource.GetDefaultView(_groupItems);

                UpdateUIControls();
            }
        }

        private void UpdateUIControls()
        {
            if (_selectedGroup == null)
                return;

            _isUpdatingControls = true;

            try
            {
                nudGroupIndex.Maximum = Math.Max(0, _groupList.Count - 1);
                nudGroupIndex.Value = _selectedGroup.Index;
                txtGroupName.Text = _selectedGroup.Name;
                chkEnabled.IsChecked = _selectedGroup.Enabled;
                chkStatic.IsChecked = _selectedGroup.IsStatic;
                chkNotify.IsChecked = _selectedGroup.Notify;
                nudNotifyTime.Value = _selectedGroup.NotTime;
            }
            finally
            {
                _isUpdatingControls = false;
            }
        }

        private void AddGroup()
        {
            var groupName = txtNewGroupName.Text.Trim();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                NotificationsShared.Error("[Filters] Please enter a group name.");
                return;
            }

            if (_groupList.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)))
            {
                NotificationsShared.Error("[Filters] A group with this name already exists.");
                return;
            }

            var index = _groupList.Count == 0 ? 0 : _groupList.Max(g => g.Index) + 1;
            var newGroup = new LootFilterGroup
            {
                Index = index,
                Name = groupName,
                Enabled = true,
                IsStatic = true
            };

            _groupList.Add(newGroup);
            LootFilterManager.CurrentGroups.Groups.Add(newGroup);
            LootFilterManager.Save();

            txtNewGroupName.Text = "";

            var tempList = new List<LootFilterGroup>(_groupList);
            cboLootFilters.ItemsSource = null;
            cboLootFilters.ItemsSource = tempList;
            cboLootFilters.SelectedItem = newGroup;

            nudGroupIndex.Maximum = Math.Max(0, _groupList.Count - 1);
        }

        private void RemoveGroup()
        {
            if (_selectedGroup == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove the group '{_selectedGroup.Name}'?",
                "Confirm Removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var groupToRemove = _selectedGroup;

                if (_groupList.Count > 1)
                {
                    int currentIndex = cboLootFilters.SelectedIndex;
                    int newIndex = (currentIndex > 0) ? currentIndex - 1 : 0;

                    if (currentIndex == 0 && _groupList.Count > 1)
                        newIndex = 1;

                    cboLootFilters.SelectedIndex = newIndex;
                }

                _groupList.Remove(groupToRemove);

                LootFilterManager.CurrentGroups.Groups.Remove(groupToRemove);

                if (_groupList.Count == 0)
                    EnsureDefaultLootFilter();

                RenumberGroupIndices();
                LootFilterManager.Save();

                var tempList = new List<LootFilterGroup>(_groupList);
                cboLootFilters.ItemsSource = null;
                cboLootFilters.ItemsSource = tempList;

                if (_groupList.Count > 0 && cboLootFilters.SelectedItem == null)
                    cboLootFilters.SelectedIndex = 0;

                nudGroupIndex.Maximum = Math.Max(0, _groupList.Count - 1);
            }
        }

        private void ToggleFilter()
        {
            if (_isUpdatingControls || _selectedGroup == null)
                return;

            _selectedGroup.Enabled = chkEnabled.IsChecked ?? false;
            SaveLootFilter();
        }

        private void ToggleStatic()
        {
            if (_isUpdatingControls || _selectedGroup == null)
                return;

            _selectedGroup.IsStatic = chkStatic.IsChecked ?? true;
            SaveLootFilter();
        }

        private void ToggleNotifications()
        {
            if (_isUpdatingControls || _selectedGroup == null)
                return;

            _selectedGroup.Notify = chkNotify.IsChecked ?? false;
            SaveLootFilter();
        }

        private void RenumberGroupIndices()
        {
            var sortedGroups = _groupList.OrderBy(g => g.Index).ToList();

            for (int i = 0; i < sortedGroups.Count; i++)
            {
                sortedGroups[i].Index = i;
            }
        }

        private void ResortGroups(LootFilterGroup groupToKeepSelected = null)
        {
            var groupToSelect = groupToKeepSelected ?? _selectedGroup;
            var sortedList = _groupList.OrderBy(g => g.Index).ToList();

            _groupList.Clear();

            foreach (var group in sortedList)
            {
                _groupList.Add(group);
            }

            LootFilterManager.CurrentGroups.Groups.Clear();
            LootFilterManager.CurrentGroups.Groups.AddRange(sortedList);

            var tempCbo = cboLootFilters;
            tempCbo.ItemsSource = null;
            tempCbo.ItemsSource = _groupList;

            if (groupToSelect != null)
                tempCbo.SelectedItem = groupToSelect;
        }

        private void AddItemToGroup()
        {
            if (_selectedGroup == null || cboItems.SelectedValue is not string itemID)
                return;

            var itemName = EftDataManager.AllItems.TryGetValue(itemID, out var item) ? item.ShortName : cboItems.Text;
            var existsInGroup = _selectedGroup.Items.Any(i => i.ItemID == itemID);
            var otherGroupWithItem = LootFilterManager.CurrentGroups.Groups
                .FirstOrDefault(g => g.Name != _selectedGroup.Name && g.Items.Any(i => i.ItemID == itemID));

            if (existsInGroup)
            {
                NotificationsShared.Info($"[Filters] {itemName} already exists in the current group: {_selectedGroup.Name}");
                return;
            }

            if (otherGroupWithItem != null)
            {
                var result = MessageBox.Show(
                    $"{itemName} already exists in another group: {otherGroupWithItem.Name}. Add it anyway?",
                    "Item Already Exists",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            NotificationsShared.Info($"[Filters] Adding {itemName} to group: {_selectedGroup.Name}");

            var entry = new GroupedLootFilterEntry
            {
                ItemID = itemID,
                Enabled = true,
                Color = "#FF2CF243",
                IsStatic = _selectedGroup.IsStatic
            };

            _selectedGroup.Items.Add(entry);
            _groupItems.Add(entry);

            entry.PropertyChanged += OnItemChanged;

            LootFilterManager.Save();

            cboItems.SelectedIndex = -1;
            txtItemSearch.Text = "";
        }

        private void RemoveItemFromGroup()
        {
            if (_selectedGroup == null)
                return;

            if (GroupedItemsListView.SelectedItem is GroupedLootFilterEntry selectedEntry)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{selectedEntry.Name}' from this group?",
                    "Confirm Removal",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedEntry.PropertyChanged -= OnItemChanged;

                    _groupItems.Remove(selectedEntry);
                    _selectedGroup.Items.Clear();
                    _selectedGroup.Items.AddRange(_groupItems);

                    LootFilterManager.Save();
                    NotificationsShared.Info($"[Filters] Removed {selectedEntry.Name} from group: {_selectedGroup.Name}");
                }
            }
            else
            {
                NotificationsShared.Warning("[Filters] Please select an item to remove");
            }
        }

        private void SearchForItem()
        {
            var query = txtItemSearch.Text.ToLowerInvariant();
            var filteredItems = EftDataManager.AllItems?
                                .Where(kv => kv.Value?.Name?.ToLowerInvariant().Contains(query) == true)
                                .OrderBy(kv => kv.Value?.Name)
                                .Take(10)
                                .Select(kv => new { Id = kv.Key, Name = kv.Value?.Name ?? "NULL" })
                                .ToList();

            cboItems.DisplayMemberPath = "Name";
            cboItems.SelectedValuePath = "Id";
            cboItems.ItemsSource = filteredItems;

            if (filteredItems != null && filteredItems.Count > 0)
                cboItems.SelectedIndex = 0;

            if (_groupItemsView != null)
            {
                _groupItemsView.Filter = item => item is GroupedLootFilterEntry entry && (string.IsNullOrWhiteSpace(query) || entry.Name.ToLowerInvariant().Contains(query));
                _groupItemsView.Refresh();
            }
        }

        private void EditGroupName()
        {
            if (_isUpdatingControls || _selectedGroup == null)
                return;

            _selectedGroup.Name = txtGroupName.Text;

            SaveLootFilter();

            var selectedItem = cboLootFilters.SelectedItem;
            cboLootFilters.Items.Refresh();
            cboLootFilters.SelectedItem = selectedItem;
        }

        private void EditFilterIndex(int value)
        {
            if (_isUpdatingControls || _selectedGroup == null)
                return;

            var newIndex = value;
            var oldIndex = _selectedGroup.Index;

            if (newIndex != oldIndex)
            {
                LoneLogging.WriteLine($"[Filters] Changing group '{_selectedGroup.Name}' index from {oldIndex} to {newIndex}");

                _selectedGroup.Index = -1;

                var allGroups = _groupList.ToList();

                foreach (var group in allGroups)
                {
                    if (group == _selectedGroup)
                        continue;

                    if (newIndex > oldIndex)
                    {
                        if (group.Index > oldIndex && group.Index <= newIndex)
                        {
                            group.Index--;
                            LoneLogging.WriteLine($"[Filters] Adjusted group '{group.Name}' index to {group.Index}");
                        }
                    }
                    else
                    {
                        if (group.Index >= newIndex && group.Index < oldIndex)
                        {
                            group.Index++;
                            LoneLogging.WriteLine($"[Filters] Adjusted group '{group.Name}' index to {group.Index}");
                        }
                    }
                }

                _selectedGroup.Index = newIndex;

                ResortGroups(_selectedGroup);

                LootFilterManager.Save();
            }
        }

        private void EditNotificationTime(int value)
        {
            if (_isUpdatingControls || _selectedGroup == null)
                return;

            _selectedGroup.NotTime = value;

            SaveLootFilter();
        }

        private void RefreshAllLootItems() => LootItem.ClearPaintCache();

        private void SaveLootFilter()
        {
            if (_selectedGroup != null)
            {
                _selectedGroup.Items.Clear();
                _selectedGroup.Items.AddRange(_groupItems);
                LootItem.ClearPaintCache();
                LootFilterManager.Save();
            }
        }

        private void RefreshGroupsComboBox(LootFilterGroup selectedGroup = null)
        {
            if (selectedGroup == null)
                selectedGroup = cboLootFilters.SelectedItem as LootFilterGroup;

            cboLootFilters.ItemsSource = null;
            cboLootFilters.ItemsSource = _groupList;

            if (selectedGroup != null && _groupList.Contains(selectedGroup))
                cboLootFilters.SelectedItem = selectedGroup;
            else if (_groupList.Count > 0)
                cboLootFilters.SelectedIndex = 0;

            nudGroupIndex.Maximum = Math.Max(0, _groupList.Count - 1);
        }

        private void OpenContextMenu()
        {
            var btn = btnMenu;

            if (btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void ExportSelectedFilter()
        {
            if (_selectedGroup == null)
            {
                NotificationsShared.Warning("[Filters] No filter selected for export.");
                return;
            }

            try
            {
                var jsonData = JsonSerializer.Serialize(new { groups = new List<LootFilterGroup> { _selectedGroup } },
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });

                Clipboard.SetText(jsonData);
                NotificationsShared.Success($"[Filters] Exported '{_selectedGroup.Name}' to clipboard");
            }
            catch (Exception ex)
            {
                NotificationsShared.Error($"[Filters] Failed to export filter to clipboard: {ex.Message}");
                LoneLogging.WriteLine($"[Filters] Export error: {ex}");
            }
        }

        private void ExportAllFilters()
        {
            if (_groupList.Count == 0)
            {
                NotificationsShared.Warning("[Filters] No filters to export.");
                return;
            }

            try
            {
                var exportData = new
                {
                    groups = _groupList.ToList(),
                    exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalGroups = _groupList.Count
                };

                var jsonData = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });

                Clipboard.SetText(jsonData);
                NotificationsShared.Success($"[Filters] Exported {_groupList.Count} filters to clipboard");
            }
            catch (Exception ex)
            {
                NotificationsShared.Error($"[Filters] Failed to export filters to clipboard: {ex.Message}");
                LoneLogging.WriteLine($"[Filters] Export error: {ex}");
            }
        }

        private void ImportFiltersFromClipboard()
        {
            try
            {
                if (!Clipboard.ContainsText())
                {
                    NotificationsShared.Warning("[Filters] Clipboard does not contain text data.");
                    return;
                }

                var clipboardText = Clipboard.GetText();
                var importData = JsonSerializer.Deserialize<dynamic>(clipboardText);

                JsonElement groupsElement;
                List<LootFilterGroup> groups = null;

                if (importData is JsonElement element && element.TryGetProperty("groups", out groupsElement))
                {
                    groups = JsonSerializer.Deserialize<List<LootFilterGroup>>(groupsElement.GetRawText());
                }
                else
                {
                    try
                    {
                        groups = JsonSerializer.Deserialize<List<LootFilterGroup>>(clipboardText);
                    }
                    catch
                    {
                        NotificationsShared.Error("[Filters] Invalid filter data in clipboard.");
                        return;
                    }
                }

                if (groups == null || groups.Count == 0)
                {
                    NotificationsShared.Error("[Filters] No valid filter groups found in clipboard.");
                    return;
                }

                var result = MessageBox.Show(
                    $"Found {groups.Count} filter group(s) in clipboard.\n\n" +
                    "Do you want to:\n" +
                    "YES: Add these filters to your existing ones\n" +
                    "NO: Replace all existing filters with the imported ones\n" +
                    "CANCEL: Cancel the import operation",
                    "Import Options",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;

                var addedCount = 0;
                var replacedCount = 0;

                if (result == MessageBoxResult.No)
                {
                    _groupList.Clear();
                    LootFilterManager.CurrentGroups.Groups.Clear();

                    foreach (var group in groups)
                    {
                        group.Index = _groupList.Count;
                        _groupList.Add(group);
                        LootFilterManager.CurrentGroups.Groups.Add(group);
                        replacedCount++;
                    }
                }
                else
                {
                    foreach (var importedGroup in groups)
                    {
                        var existingGroup = _groupList.FirstOrDefault(g => g.Name.Equals(importedGroup.Name, StringComparison.OrdinalIgnoreCase));

                        if (existingGroup != null)
                        {
                            var replaceResult = MessageBox.Show(
                                $"A filter group named '{importedGroup.Name}' already exists.\n" +
                                "Do you want to replace it?",
                                "Duplicate Filter Group",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (replaceResult == MessageBoxResult.Yes)
                            {
                                importedGroup.Index = existingGroup.Index;
                                var index = _groupList.IndexOf(existingGroup);
                                _groupList[index] = importedGroup;
                                LootFilterManager.CurrentGroups.Groups[index] = importedGroup;
                                replacedCount++;
                            }
                        }
                        else
                        {
                            importedGroup.Index = _groupList.Count == 0 ? 0 : _groupList.Max(g => g.Index) + 1;
                            _groupList.Add(importedGroup);
                            LootFilterManager.CurrentGroups.Groups.Add(importedGroup);
                            addedCount++;
                        }
                    }
                }

                if (_groupList.Count == 0)
                    EnsureDefaultLootFilter();

                RefreshGroupsComboBox();
                RenumberGroupIndices();
                LootFilterManager.Save();

                var message = result == MessageBoxResult.No
                    ? $"[Filters] Replaced all filters. Imported {replacedCount} group(s)."
                    : $"[Filters] Added {addedCount} new group(s), replaced {replacedCount} existing group(s).";

                NotificationsShared.Success(message);
            }
            catch (Exception ex)
            {
                NotificationsShared.Error($"[Filters] Failed to import filters from clipboard: {ex.Message}");
                LoneLogging.WriteLine($"[Filters] Import error: {ex}");
            }
        }

        public static LootFilterGroup CreateWeaponAmmoGroup()
        {
            LoneLogging.WriteLine("[Filters] Creating dynamic group for current weapon ammo.");

            var groupName = Memory?.LocalPlayer.Hands.CurrentItem;
            var weaponBsgId = Memory?.LocalPlayer.Hands.CurrentItemId;

            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(weaponBsgId))
            {
                LoneLogging.WriteLine("[Filters] Weapon name or ID is null. Skipping group creation.");
                return null;
            }

            if (!EftDataManager.AllItems.TryGetValue(weaponBsgId, out var weaponItem) ||
                string.IsNullOrEmpty(weaponItem.Caliber))
            {
                LoneLogging.WriteLine($"[Filters] Caliber not found for weapon ID: {weaponBsgId}");
                return null;
            }

            var caliber = weaponItem.Caliber;
            var matchingAmmoIds = AmmoLookup.GetCompatibleAmmo(caliber);

            if (LootFilterManager.CurrentGroups.Groups.Any(g => g.Name == groupName))
            {
                LoneLogging.WriteLine($"[Filters] Group '{groupName}' already exists. Skipping creation.");
                return null;
            }

            var dynamicGroup = new LootFilterGroup
            {
                Name = groupName,
                Index = LootFilterManager.CurrentGroups.Groups.Count == 0
                    ? 0
                    : LootFilterManager.CurrentGroups.Groups.Max(g => g.Index) + 1,
                Enabled = true,
                IsStatic = false,
                Items = matchingAmmoIds
                    .Where(id => EftDataManager.AllItems.ContainsKey(id))
                    .Select(id => new GroupedLootFilterEntry
                    {
                        ItemID = id,
                        Enabled = true,
                        Color = "#FF2CF243",
                        IsStatic = false
                    })
                    .ToList()
            };

            MainWindow.Window.LootFilterControl._groupList.Add(dynamicGroup);
            LootFilterManager.CurrentGroups.Groups.Add(dynamicGroup);

            LootFilterManager.Save();

            var lootFilterSettings = MainWindow.Window.LootFilterControl;
            lootFilterSettings.RefreshGroupsComboBox(dynamicGroup);

            LoneLogging.WriteLine($"[Filters] Created dynamic group: {groupName} with {dynamicGroup.Items.Count} items.");
            return dynamicGroup;
        }

        public static void RemoveNonStaticGroups()
        {
            var lootFilterSettings = MainWindow.Window.LootFilterControl;
            var nonStaticGroups = LootFilterManager.CurrentGroups.Groups
                .Where(g => g.IsStatic == false)
                .ToList();

            var needsRefresh = false;
            var selectedWillBeRemoved = lootFilterSettings._selectedGroup != null &&
                                        !lootFilterSettings._selectedGroup.IsStatic;

            foreach (var group in nonStaticGroups)
            {
                lootFilterSettings._groupList.Remove(group);
                LootFilterManager.CurrentGroups.Groups.Remove(group);
                LoneLogging.WriteLine($"[Filters] Removed non-static group: {group.Name}");
                needsRefresh = true;
            }

            LoneLogging.WriteLine($"[Filters] Removed {nonStaticGroups.Count} non-static groups.");
            firstRemove = true;

            if (needsRefresh)
            {
                if (lootFilterSettings._groupList.Count == 0)
                {
                    lootFilterSettings.EnsureDefaultLootFilter();
                }
                else if (selectedWillBeRemoved)
                {
                    lootFilterSettings.cboLootFilters.SelectedIndex = 0;
                }

                lootFilterSettings.RenumberGroupIndices();

                LootFilterManager.Save();

                lootFilterSettings.RefreshGroupsComboBox();
            }
        }

        public static Predicate<LootItem> Create()
        {
            var search = SearchString?.Trim();
            var usePrices = string.IsNullOrEmpty(search);

            if (usePrices)
            {
                Predicate<LootItem> p = (x) =>
                {
                    return (x.IsRegularLoot || x.IsValuableLoot || x.IsImportant || x.IsWishlisted) ||
                            (ShowQuestItems && x.IsQuestCondition) ||
                            (ShowBackpacks && x.IsBackpack) ||
                            (ShowMeds && x.IsMeds) ||
                            (ShowFood && x.IsFood);
                };
                return (item) =>
                {
                    if (item is LootAirdrop || item is LootCorpse)
                        return true;

                    if (p(item))
                    {
                        if (item is LootContainer container)
                            container.SetFilter(p);

                        return true;
                    }

                    return false;
                };
            }
            else
            {
                var names = search?.Split(',').Select(a => a.Trim()).ToArray() ?? Array.Empty<string>();

                Predicate<LootItem> p = (x) =>
                {
                    return names.Any(a => x.Name.Contains(a, StringComparison.OrdinalIgnoreCase));
                };

                return (item) =>
                {
                    if (item is LootAirdrop || item.ContainsSearchPredicate(p))
                    {
                        if (item is LootContainer container)
                            container.SetFilter(p);

                        return true;
                    }

                    return false;
                };
            }
        }
        #endregion

        #region Events
        private void LootFilterCheckbox_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.Tag is string tag)
            {
                switch (tag)
                {
                    case "RemoveGroup":
                        RemoveGroup();
                        break;
                    case "AddGroup":
                        AddGroup();
                        break;
                    case "ToggleFilter":
                        ToggleFilter();
                        break;
                    case "StaticFilter":
                        ToggleStatic();
                        break;
                    case "FilterNotifications":
                        ToggleNotifications();
                        break;
                    case "AddItem":
                        AddItemToGroup();
                        break;
                    case "RemoveItem":
                        RemoveItemFromGroup();
                        break;
                }
            }
        }

        private void LootFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                switch (tag)
                {
                    case "ContextMenu":
                        OpenContextMenu();
                        break;
                    case "RemoveGroup":
                        RemoveGroup();
                        break;
                    case "AddGroup":
                        AddGroup();
                        break;
                    case "AddItem":
                        AddItemToGroup();
                        break;
                    case "RemoveItem":
                        RemoveItemFromGroup();
                        break;
                }
            }
        }

        private void LootFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is HandyControl.Controls.TextBox txt && txt.Tag is string tag)
            {
                switch (tag)
                {
                    case "ItemSearch":
                        SearchForItem();
                        break;
                    case "GroupName":
                        EditGroupName();
                        break;
                }
            }
        }

        private void LootFilterNumericUpDown_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (sender is HandyControl.Controls.NumericUpDown nud && nud.Tag is string tag)
            {
                var value = (int)e.Info;

                switch (tag)
                {
                    case "GroupIndex":
                        EditFilterIndex(value);
                        break;
                    case "NotificationTime":
                        EditNotificationTime(value);
                        break;
                }
            }
        }

        private void cboLootFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingControls || cboLootFilters.SelectedItem == null)
                return;

            var selectedGroup = cboLootFilters.SelectedItem as LootFilterGroup;

            if (selectedGroup != null)
            {
                LoneLogging.WriteLine($"[Filters] Selected group changed to: {selectedGroup.Name}, Index: {selectedGroup.Index}");
                SetSelectedGroup(selectedGroup);
            }
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GroupedLootFilterEntry entry)
            {
                _openColorPicker?.Close();

                var originalColor = (Color)ColorConverter.ConvertFromString(entry.Color);
                var picker = SingleOpenHelper.CreateControl<ColorPicker>();
                picker.SelectedBrush = new SolidColorBrush(originalColor);
                var window = new PopupWindow
                {
                    PopupElement = picker,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    MinWidth = 0,
                    MinHeight = 0
                };

                _openColorPicker = window;

                var parentWindow = MainWindow.GetWindow(btn);
                var lootFilterPanel = parentWindow?.FindName("LootFilterPanel") as FrameworkElement;

                void UpdatePickerPosition()
                {
                    try
                    {
                        var buttonPos = btn.PointToScreen(new Point(0, 0));
                        var leftPos = buttonPos.X + btn.ActualWidth - 5;
                        var topPos = buttonPos.Y - btn.ActualHeight - 5;

                        window.Left = leftPos;
                        window.Top = topPos;
                    }
                    catch { }
                }

                EventHandler parentLocationChanged = (s, e) => UpdatePickerPosition();
                SizeChangedEventHandler parentSizeChanged = (s, e) => UpdatePickerPosition();
                EventHandler panelLayoutUpdated = (s, e) => UpdatePickerPosition();

                if (parentWindow != null)
                {
                    parentWindow.LocationChanged += parentLocationChanged;
                    parentWindow.SizeChanged += parentSizeChanged;
                }

                if (lootFilterPanel != null)
                {
                    lootFilterPanel.LayoutUpdated += panelLayoutUpdated;
                }

                picker.Confirmed += (s, args) =>
                {
                    if (picker.SelectedBrush is SolidColorBrush scb)
                    {
                        entry.Color = scb.Color.ToString();
                        btn.Background = scb;
                        SaveLootFilter();
                    }
                    window.Close();
                };

                picker.Canceled += (s, args) =>
                {
                    window.Close();
                };

                window.Loaded += (s, args) =>
                {
                    UpdatePickerPosition();
                };

                window.Closed += (s, args) =>
                {
                    _openColorPicker = null;

                    if (parentWindow != null)
                    {
                        parentWindow.LocationChanged -= parentLocationChanged;
                        parentWindow.SizeChanged -= parentSizeChanged;
                    }

                    if (lootFilterPanel != null)
                    {
                        lootFilterPanel.LayoutUpdated -= panelLayoutUpdated;
                    }
                };

                window.Show(btn, false);
            }
        }

        private void OnGroupChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveLootFilter();
            RefreshAllLootItems();
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveLootFilter();
            RefreshAllLootItems();
        }

        private void NotifyIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is GroupedLootFilterEntry item)
            {
                item.Notify = !item.Notify;
                e.Handled = true;
            }
        }

        private void EnabledIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is GroupedLootFilterEntry item)
            {
                item.Enabled = !item.Enabled;
                e.Handled = true;
            }
        }

        private void LootFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mnu && mnu.Tag is string tag)
            {
                switch (tag)
                {
                    case "ExportSelectedFilter":
                        ExportSelectedFilter();
                        break;
                    case "ExportAllFilters":
                        ExportAllFilters();
                        break;
                    case "ImportFilters":
                        ImportFiltersFromClipboard();
                        break;
                }
            }
        }
        #endregion
        #endregion
    }
}