using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Serilog.Events;
using Serilog.Sinks.WinUi3.LogViewModels;
using Serilog.Sinks.WinUi3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace HelpChat.Utils
{
    public class ItemsRepeaterLogBroker : IWinUi3LogBroker
    {
        private readonly ILogViewModelBuilder _logViewModelBuilder;

        public ItemsRepeaterLogBroker(
            ItemsRepeater itemsRepeater,
            ScrollViewer scrollViewer,
            ILogViewModelBuilder logViewModelBuilder)
        {
            itemsRepeater.SetBinding(ItemsRepeater.ItemsSourceProperty, new Binding() { Source = Logs });

            _logViewModelBuilder = logViewModelBuilder;

            DispatcherQueue = itemsRepeater.DispatcherQueue;
            AddLogEvent = logEvent => Logs.Add(_logViewModelBuilder.Build(logEvent));
            Logs.CollectionChanged += ((sender, e) =>
            {
                if (IsAutoScrollOn is true && sender is ObservableCollection<ILogViewModel> collection)
                {
                    scrollViewer.ChangeView(
                        horizontalOffset: 0,
                        verticalOffset: scrollViewer.ScrollableHeight,
                        zoomFactor: 1,
                        disableAnimation: true);
                }
            });
        }

        public Action<LogEvent> AddLogEvent { get; }
        public DispatcherQueue DispatcherQueue { get; }
        public bool IsAutoScrollOn { get; set; }

        private ObservableCollection<ILogViewModel> Logs { get; init; } = new();

    }
}
