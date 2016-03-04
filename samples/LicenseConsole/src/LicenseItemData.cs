using System;
using System.Diagnostics;
using System.Windows;

namespace LicenseConsole
{
    public class LicenseItemData : DependencyObject
    {
        private static readonly DependencyPropertyKey NamePropertyKey = DependencyProperty.RegisterReadOnly(
            "Name", typeof(string), typeof(LicenseItemData), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty NameProperty = NamePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DescriptionPropertyKey = DependencyProperty.RegisterReadOnly(
            "Description", typeof(string), typeof(LicenseItemData), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty DescriptionProperty = DescriptionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State", typeof(LicenseItemState), typeof(LicenseItemData), new FrameworkPropertyMetadata(LicenseItemState.Blocked));

        public LicenseItemData(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            private set { SetValue(NamePropertyKey, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            private set { SetValue(DescriptionPropertyKey, value); }
        }

        public LicenseItemState State
        {
            get { return (LicenseItemState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
    }
}
