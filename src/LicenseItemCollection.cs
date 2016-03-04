using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Represents a collection of <see cref="LicenseItem"/> objects.</summary>
    public sealed class LicenseItemCollection : ObservableCollection<LicenseItem>
    {
        private License _licence;

        internal LicenseItemCollection(License license)
        {
            Debug.Assert(license != null);
            _licence = license;
        }

        /// <summary>Gets the containing <see cref="License" />.</summary>
        /// <value>The <see cref="License" /> object contains this <see cref="LicenseItemCollection" />.</value>
        public License License
        {
            get { return _licence; }
        }

        /// <summary>Gets the license item for specified name.</summary>
        /// <param name="name">The specified license item name.</param>
        /// <value>A <see cref="LicenseItem"/> object matches the specified license item name. <see langword="null" /> if
        /// no <see cref="LicenseItem" /> found.</value>
        public LicenseItem this[string name]
        {
            get
            {
                foreach (LicenseItem item in this)
                {
                    if (item.Name == name)
                        return item;
                }
                return null;
            }
        }

        /// <exclude />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            License.VerifyFrozenAccess();

            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (LicenseItem item in e.OldItems)
                        item.Freeze(null);
                }
            }

            if (e.Action != NotifyCollectionChangedAction.Move &&
                e.Action != NotifyCollectionChangedAction.Reset)
            {
                if (e.NewItems != null)
                {
                    foreach (LicenseItem item in e.NewItems)
                        item.Freeze(_licence);
                }

                CheckNames();
            }

            base.OnCollectionChanged(e);
        }

        private List<string> _list = new List<string>();
        private void CheckNames()
        {
            Debug.Assert(_list.Count == 0);

            foreach (LicenseItem item in this)
            {
                if (string.IsNullOrEmpty(item.Name))
                    throw new InvalidOperationException(ExceptionMessages.FormatEmptyLicenseItemName(IndexOf(item)));

                if (_list.Contains(item.Name))
                    throw new InvalidOperationException(ExceptionMessages.FormatDuplicateLicenseItemName(item.Name));

                _list.Add(item.Name);
            }
            _list.Clear();
        }

        /// <exclude/>
        protected override void ClearItems()
        {
            foreach (LicenseItem item in this)
                item.Freeze(null);
            base.ClearItems();
        }
    }
}
