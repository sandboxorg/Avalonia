// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.UnitTests;
using Xunit;

namespace Avalonia.Markup.Xaml.UnitTests.Xaml
{
    public class DataTemplateTests
    {
        [Fact]
        public void DataTemplate_Can_Contain_Name()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
        xmlns:sys='clr-namespace:System;assembly=mscorlib'>
    <Window.DataTemplates>
        <DataTemplate DataType='{Type sys:String}'>
            <Canvas Name='foo'/>
        </DataTemplate>
    </Window.DataTemplates>
    <ContentControl Name='target' Content='Foo'/>
</Window>";
                var loader = new AvaloniaXamlLoader();
                var window = (Window)loader.Load(xaml);
                var target = window.FindControl<ContentControl>("target");

                window.ApplyTemplate();
                target.ApplyTemplate();
                ((ContentPresenter)target.Presenter).UpdateChild();

                Assert.IsType<Canvas>(target.Presenter.Child);
            }
        }

        [Fact]
        public void Can_Set_DataContext_In_DataTemplate()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
        xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests;assembly=Avalonia.Markup.Xaml.UnitTests'>
    <Window.DataTemplates>
        <DataTemplate DataType='{Type local:TestViewModel}'>
            <Canvas Name='foo' DataContext='{Binding Child}'/>
        </DataTemplate>
    </Window.DataTemplates>
    <ContentControl Name='target' Content='{Binding Child}'/>
</Window>";
                var loader = new AvaloniaXamlLoader();
                var window = (Window)loader.Load(xaml);
                var target = window.FindControl<ContentControl>("target");

                var viewModel = new TestViewModel
                {
                    String = "Root",
                    Child = new TestViewModel
                    {
                        String = "Child",
                        Child = new TestViewModel
                        {
                            String = "Grandchild",
                        }
                    },
                };

                window.DataContext = viewModel;

                window.ApplyTemplate();
                target.ApplyTemplate();
                ((ContentPresenter)target.Presenter).UpdateChild();

                var canvas = (Canvas)target.Presenter.Child;
                Assert.Same(viewModel, target.DataContext);
                Assert.Same(viewModel.Child.Child, canvas.DataContext);
            }
        }
    }
}
