using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace Tetris
{
    /// <summary>
    /// 指定されたキーが押下されるまでバックグラウンドで待ち、押下された場合にイベントを発生させる
    /// アプリケーションにフォーカスが無くてもキーの状態を取得できます
    /// </summary>
    public class KeyStateBackgroundWatcher
    {
        // Lockオブジェクト
        private object _lock = new object();

        // 確認するキーのリスト
        private List<Keys> _keyCodeList;

        // 確認間隔
        private int _interval;

        // 別スレッド管理用のTask
        private Task _task;

        // 動作フラグ
        private bool stopFlag = false;

        // GetAsyncKeyStateの押下確認用ビットマスク
        private const Int64 mask64 = (Int64)0x8000;

        // イベント
        private event EventHandler<KeyEventArgs> _callBackMethod;

        // アンマネージド キーが押下されたかどうか確認する
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetAsyncKeyState(Keys nVirtKey);

        /// <summary>
        /// 指定されたキーが押下されるまでバックグラウンドで待ち、押下された場合にイベントを発生させる
        /// </summary>
        /// <param name="interval">確認間隔 ミリ秒</param>
        /// <param name="callBackMethod">イベント</param>
        /// <param name="keyCode">確認するキーコード</param>
        public KeyStateBackgroundWatcher(int interval, EventHandler<KeyEventArgs> callBackMethod, params Keys[] keyCode)
        {
            // 毎回キャストも嫌なのでList<IntPtr>にしておく .ToList<IntPtr>()ができなかった。。
            _keyCodeList = new List<Keys>(keyCode.Length);
            //foreach (Keys x in keyCode)
            //{
            //    _keyCodeList.Add(x);
            //}
            _keyCodeList.AddRange(keyCode);

            _interval = interval;
            _callBackMethod = callBackMethod;
        }

        /// <summary>
        /// 指定されたキー状態を観察します
        /// </summary>
        public void Watch()
        {
            lock (_lock)
            {
                // 多重起動防止
                if (_task != null)
                    if (_task.Status == TaskStatus.Running)
                        return;

                // 停止フラグ
                stopFlag = true;
            }

            // 監視開始
            _task = new Task(WatchKeys);
            _task.Start();
        }

        /// <summary>
        /// 観察を中断します
        /// </summary>
        public void Abort()
        {
            lock (_lock)
            {
                stopFlag = false;
            }
        }

        /// <summary>
        /// 指定されたキーの観察
        /// </summary>
        private void WatchKeys()
        {
            System.Threading.Thread.CurrentThread.Name = "KeysListener_" + _keyCodeList[0].ToString();
            Keys pressedCode = Keys.None;
            while (stopFlag)
            {
                if (_keyCodeList.Exists(x =>
                {
                    pressedCode = x;
                    return WatchKey(x);
                }))
                    // 見つかったらコールバック
                    _callBackMethod(this, new KeyEventArgs(pressedCode));

                // 見つからない場合はウェイトする
                Thread.Sleep(_interval);
            }

        }

        /// <summary>
        /// 指定されたキーが押下されたか確認
        /// </summary>
        /// <param name="code">キーコード</param>
        /// <returns>キーの状態、押下された場合true</returns>
        private bool WatchKey(Keys code)
        {
            //IntPtr virtualcode = new IntPtr(KeyInterop.VirtualKeyFromKey(Key.Down));
            var returnValue = GetAsyncKeyState(code).ToInt64() & mask64;
            return (returnValue != 0) ? true : false;
        }
    }

}
