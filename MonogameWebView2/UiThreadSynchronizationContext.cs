﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace MonogameWebView2
{
    internal sealed class UiThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> m_queue = new();
        private readonly HWND hwnd;

        public UiThreadSynchronizationContext(HWND hwnd) : base()
        {
            this.hwnd = hwnd;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            //PInvoke.PostMessage(hwnd, WindowsWebView.WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE, 0, 0);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            m_queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            //PInvoke.SendMessage(hwnd, WindowsWebView.WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE, 0, 0);
        }

        public void RunAvailableWorkOnCurrentThread()
        {
            while (m_queue.TryTake(out KeyValuePair<SendOrPostCallback, object> workItem))
                workItem.Key(workItem.Value);
        }

        public void Complete() { m_queue.CompleteAdding(); }
    }
}
