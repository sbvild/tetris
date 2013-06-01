using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tetris
{

    public delegate void TetrisUpdateDelegate(TetrisEventArgs args);

    class Tetris
    {
        private class Sync
        {

           //
            private static string user = "";

            public static void Lock(string id)
            {
                //
                lock (user)
                {
                    //
                    while (user != "")
                    {
                        //
                        System.Threading.Thread.Sleep(1);
                    }
                    user = id;

                }
            }

            public static void UnLock()
            {
                //
                user = "";
            }
        }

        private class Const
        {
            //
            public const int UPDATE_PERIOD = 1000; //テトリスが落ちてくる間隔
            public const int ERASE_DUETIME = 500;  //消去時→次のテトリスが落ちてくる間隔
        }

        private class Defaults
        {
            //

            public const int STAGE_HEIGHT = 20;
            public const int STAGE_WIDTH = 11;
        }

        private Timer m_timer;  //ループタイマー
        private Stage m_stage;  //テトリスの論理データ + 描画クラス
        private TetrisStatus m_status;  //テトリスの状態

        public event TetrisUpdateDelegate Updated;

        public TetrisStatus Status
        {
            get
            {
                return m_status;
            }
        }
        public IStage Stage
        {
            get
            {
                return m_stage;
            }
        }
        private Stage _Stage
        {
            get
            {
                    return m_stage;
            }
        }

        public Tetris() : this(Defaults.STAGE_WIDTH,Defaults.STAGE_HEIGHT)
        {
           //
        }
        public Tetris(int stage_Width, int stage_Height)
        {
            m_stage = new Stage(stage_Width, stage_Height);
            m_status = TetrisStatus.NOTSTART;
        }

        public void Start()
        {
            if (! (m_status == TetrisStatus.NOTSTART))
                return;

            lock (this)
            {
                m_timer = new Timer(Loop, null, 0, Const.UPDATE_PERIOD);
                m_status = TetrisStatus.READY;
            }
            //Loop(null);
        }

  
        private void Loop(object o)
        {

            Sync.Lock("GameLoop");

            try
            {
                if (m_status == TetrisStatus.READY)
                    m_status = TetrisStatus.CONTINUE;

                //揃った!
                if (!_Stage.HasMovablePiece() && _Stage.HasLinkedLines())
                {
                    //
                    _Stage.EraseLines();
                    //Update();

                    m_timer.Change(Const.ERASE_DUETIME, Const.UPDATE_PERIOD);
                    //下にどーん
                    //if ((!m_stage.HasMovablePiece() && m_stage.HasBlankedLines()))
                    //{
                    //
                    _Stage.HardDropAll();
                    //Update();

                    //}
                    return;
                }

                //次のテトリス
                if (!_Stage.HasMovablePiece())
                {
                    _Stage.AddPiece();
                    //Update();
                    return;
                }

                //TODO:
                //1fps単位に描画
                _Stage.MovePiece(Direction.Bottom);
                //Update();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                //
                Update();
                Sync.UnLock();
            }


        }

        public void MovePiece(Direction direction)
        {
            try
            {
                //
                Sync.Lock("KyesListener");

                if (Status == TetrisStatus.NOTSTART)
                    return;
                _Stage.MovePiece(direction);
            }
            finally
            {
                Update();
                Sync.UnLock();
            }

        }

        public void RotatePiece(Direction direction)
        {
            try
            {
                //
                Sync.Lock("KyesListener");

                if (Status == TetrisStatus.NOTSTART)
                    return;

                _Stage.RotatePiece(direction);
            }
            finally
            {
                //
                Update();
                Sync.UnLock();
            }


        }

        private void Update()
        {
            //継続可能？
            if (CanContinue())
            {
                Updated(new TetrisEventArgs(TetrisStatus.CONTINUE, _Stage));
            }
            else
            {
                m_timer.Dispose();
                Updated(new TetrisEventArgs(TetrisStatus.GAMEOVER, _Stage));
            }
        }

        private bool CanContinue()
        {
            return true;
        }


    }


    public class TetrisEventArgs : EventArgs
    {
        private TetrisStatus m_status;
        private Stage m_stage;

        public TetrisStatus Status
        {
            get
            {
                return m_status;
            }
        }
        public Stage Stage
        {
            get
            {
                return m_stage;
            }
        }

        public TetrisEventArgs(TetrisStatus status, Stage stage)
        {
            m_status = status;
            m_stage = stage;
        }

    }

    public enum TetrisStatus
    {
        NOTSTART,
        READY,
        CONTINUE,
        GAMEOVER
    }



}
