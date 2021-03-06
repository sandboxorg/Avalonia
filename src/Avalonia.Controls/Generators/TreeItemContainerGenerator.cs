// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace Avalonia.Controls.Generators
{
    /// <summary>
    /// Creates containers for tree items and maintains a list of created containers.
    /// </summary>
    /// <typeparam name="T">The type of the container.</typeparam>
    public class TreeItemContainerGenerator<T> : ItemContainerGenerator<T>, ITreeItemContainerGenerator
        where T : class, IControl, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeItemContainerGenerator{T}"/> class.
        /// </summary>
        /// <param name="owner">The owner control.</param>
        /// <param name="contentProperty">The container's Content property.</param>
        /// <param name="contentTemplateProperty">The container's ContentTemplate property.</param>
        /// <param name="itemsProperty">The container's Items property.</param>
        /// <param name="isExpandedProperty">The container's IsExpanded property.</param>
        /// <param name="index">The container index for the tree</param>
        public TreeItemContainerGenerator(
            IControl owner,
            AvaloniaProperty contentProperty,
            AvaloniaProperty contentTemplateProperty,
            AvaloniaProperty itemsProperty,
            AvaloniaProperty isExpandedProperty,
            TreeContainerIndex index)
            : base(owner, contentProperty, contentTemplateProperty)
        {
            Contract.Requires<ArgumentNullException>(owner != null);
            Contract.Requires<ArgumentNullException>(contentProperty != null);
            Contract.Requires<ArgumentNullException>(itemsProperty != null);
            Contract.Requires<ArgumentNullException>(isExpandedProperty != null);
            Contract.Requires<ArgumentNullException>(index != null);

            ItemsProperty = itemsProperty;
            IsExpandedProperty = isExpandedProperty;
            Index = index;
        }

        /// <summary>
        /// Gets the container index for the tree.
        /// </summary>
        public TreeContainerIndex Index { get; }

        /// <summary>
        /// Gets the item container's Items property.
        /// </summary>
        protected AvaloniaProperty ItemsProperty { get; }

        /// <summary>
        /// Gets the item container's IsExpanded property.
        /// </summary>
        protected AvaloniaProperty IsExpandedProperty { get; }

        /// <inheritdoc/>
        protected override IControl CreateContainer(object item)
        {
            var container = item as T;

            if (item == null)
            {
                return null;
            }
            else if (container != null)
            {
                Index.Add(item, container);
                return container;
            }
            else
            {
                var template = GetTreeDataTemplate(item, ItemTemplate);
                var result = new T();

                result.SetValue(ContentProperty, template.Build(item));

                var itemsSelector = template.ItemsSelector(item);

                if (itemsSelector != null)
                {
                    BindingOperations.Apply(result, ItemsProperty, itemsSelector, null);
                }

                if (!(item is IControl))
                {
                    result.DataContext = item;
                }

                NameScope.SetNameScope((Control)(object)result, new NameScope());
                Index.Add(item, result);

                return result;
            }
        }

        public override IEnumerable<ItemContainer> Clear()
        {
            var items = base.Clear();
            Index.Remove(items);
            return items;
        }

        public override IEnumerable<ItemContainer> Dematerialize(int startingIndex, int count)
        {
            Index.Remove(GetContainerRange(startingIndex, count));
            return base.Dematerialize(startingIndex, count);
        }

        public override IEnumerable<ItemContainer> RemoveRange(int startingIndex, int count)
        {
            Index.Remove(GetContainerRange(startingIndex, count));
            return base.RemoveRange(startingIndex, count);
        }

        /// <summary>
        /// Gets the data template for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The template.</returns>
        private ITreeDataTemplate GetTreeDataTemplate(object item, IDataTemplate primary)
        {
            var template = Owner.FindDataTemplate(item, primary) ?? FuncDataTemplate.Default;
            var treeTemplate = template as ITreeDataTemplate ??
                new FuncTreeDataTemplate(typeof(object), template.Build, x => null);
            return treeTemplate;
        }
    }
}
