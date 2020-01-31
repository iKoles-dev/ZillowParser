using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Homebrew
{
    public class Parser
    {
        /// <summary>
        /// Доступ к окну лога
        /// </summary>
        protected RichTextBox DebugBox = Controls.DebugBox;
        /// <summary>
        /// Доступ к прогресс бару + авто-установка его числового отображения
        /// </summary>
        protected ProgressBar WorkProgress
        {
            get
            {
                return Controls.WorkProgress;
            }
            set
            {
                if (value.Value >= 0 && value.Value <= 100)
                {
                    Controls.WorkProgressLabel.Set(value + "%");
                }
                Controls.WorkProgress = value;
            }
        }
        /// <summary>
        /// Доступ к сохранённым куки
        /// </summary>
        protected CookieContainer SavedCookies = new CookieContainer();
        protected delegate void SyncThread(string param);
        protected Stack<SyncThread> ThreadStack = new Stack<SyncThread>();
        private int threadCount = 0;
        protected delegate void ExecuteHandler();
        protected event ExecuteHandler OnExecuteCimpleted;
        protected void Execute(int threadCount)
        {
            for (int i = 0; threadCount > i; i++)
            {
                threadCount++;
                StartThread();
            }
        }
        private void StartThread()
        {
            while (ThreadStack.Count > 0)
            {
                ThreadStack.Pop();
            }
            threadCount--;
            if (threadCount == 0)
            {
                OnExecuteCimpleted.Invoke();
            }
        }
    }
}
