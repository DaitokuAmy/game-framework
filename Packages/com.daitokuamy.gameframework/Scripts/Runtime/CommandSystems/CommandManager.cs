using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンド管理クラス
    /// </summary>
    public class CommandManager : IDisposable {
        private readonly List<ICommand> _standbyCommands = new();
        private readonly List<ICommand> _executingCommands = new();
        private readonly int _maxStandbyCount;
        
        private bool _standbyDirty;

        /// <summary>実行待機中Command(Debug用)</summary>
        public IReadOnlyList<ICommand> StandbyCommands {
            get {
                if (_standbyDirty) {
                    Sort(_standbyCommands);
                    _standbyDirty = false;
                }

                return _standbyCommands;
            }
        }
        /// <summary>実行中Command(Debug用)</summary>
        public IReadOnlyList<ICommand> ExecutingCommands => _executingCommands;

        /// <summary>コマンドがスタンバイされた通知(Debug用)</summary>
        public event Action<ICommand> StandbyedCommandEvent;
        /// <summary>コマンドが実行された通知(Debug用)</summary>
        public event Action<ICommand> ExecutedCommandEvent;
        /// <summary>コマンドが除外された通知(Debug用)</summary>
        public event Action<ICommand> RemovedCommandEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxStandbyCount">待機可能コマンドの上限</param>
        public CommandManager(int maxStandbyCount = -1) {
            _maxStandbyCount = maxStandbyCount;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            ClearCommands();
        }

        /// <summary>
        /// コマンドの追加
        /// </summary>
        /// <param name="command">追加するCommand</param>
        public CommandHandle Add(ICommand command) {
            if (command == null) {
                Debug.LogWarning("command is invalid.");
                return CommandHandle.Empty;
            }

            if (command.CurrentState == CommandState.Destroyed) {
                // 廃棄済みならリサイクルする
                command.Recycle();
            }

            if (command.CurrentState != CommandState.Invalid) {
                Debug.LogWarning($"invalid command. [{command.GetType()}]");
                return CommandHandle.Empty;
            }

            _standbyCommands.Add(command);
            command.Initialize();
            StandbyedCommandEvent?.Invoke(command);
            
            // 実行中の優先度の低い物をキャンセル
            if (command.AddedCancelLowPriorityOthers) {
                CancelCommands(command.Priority - 1);
            }

            // 待機上限に入っていた場合、優先度が低い物を除外            
            if (_maxStandbyCount >= 0 && _standbyCommands.Count > _maxStandbyCount) {
                Sort(_standbyCommands);
                while (_standbyCommands.Count > _maxStandbyCount) {
                    var removeCommand = _standbyCommands[0];
                    removeCommand.Destroy();
                    _standbyCommands.RemoveAt(0);
                    RemovedCommandEvent?.Invoke(removeCommand);
                }
            }
            // 要素追加されていた場合、ソートを依頼
            else {
                _standbyDirty = true;
            }

            return new CommandHandle(command);
        }

        /// <summary>
        /// コマンドのクリア
        /// </summary>
        public void ClearCommands() {
            // 実行中のコマンドを強制終了
            for (var i = _executingCommands.Count - 1; i >= 0; i--) {
                _executingCommands[i].Destroy();
            }
            
            // 実行待機中のコマンドを強制終了
            for (var i = _standbyCommands.Count - 1; i >= 0; i--) {
                _standbyCommands[i].Destroy();
            }
            
            _executingCommands.Clear();
            _standbyCommands.Clear();
            _standbyDirty = false;
        }

        /// <summary>
        /// 特定優先度以下のCommandをキャンセルする
        /// </summary>
        /// <param name="priority">キャンセルするCommandのPriority</param>
        public void CancelCommands(int priority) {
            // 実行中のコマンドを強制終了
            for (var i = _executingCommands.Count - 1; i >= 0; i--) {
                var command = _executingCommands[i];
                if (command.Priority > priority) {
                    break;
                }
                
                _executingCommands[i].Destroy();
            }
            
            // 実行待機中のコマンドを強制終了
            for (var i = _standbyCommands.Count - 1; i >= 0; i--) {
                var command = _standbyCommands[i];
                if (command.Priority > priority) {
                    break;
                }
                
                _standbyCommands[i].Destroy();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            if (_standbyDirty) {
                Sort(_standbyCommands);
                _standbyDirty = false;
            }

            // 実行中CommandからBlockPriorityを取得
            var blockPriority = -1;
            for (var i = _executingCommands.Count - 1; i >= 0; i--) {
                var command = _executingCommands[i];
                if (command.BlockStandbyOthers) {
                    blockPriority = command.Priority;
                    break;
                }
            }

            // 待機中コマンドを実行状態に移す
            var executingDirty = false;
            for (var i = _standbyCommands.Count - 1; i >= 0; i--) {
                var command = _standbyCommands[i];
                
                // 待機中更新
                if (!command.StandbyUpdate()) {
                    command.Destroy();
                    _standbyCommands.RemoveAt(i);
                    continue;
                }

                // 実行状態に移せるか
                if (command.Priority < blockPriority) {
                    continue;
                }

                // 実行待ちを行う場合は空じゃないとやらない
                if (command.WaitExecutionOthers && _executingCommands.Count > 0) {
                    continue;
                }
                
                // 実行開始
                if (command.Start() && command.CurrentState == CommandState.Executing) {
                    _standbyCommands.RemoveAt(i);
                    _executingCommands.Add(command);
                    ExecutedCommandEvent?.Invoke(command);
                    executingDirty = true;
                    
                    // 実行中の優先度の低い物をキャンセル
                    if (command.ExecutedCancelLowPriorityOthers) {
                        CancelCommands(command.Priority - 1);
                    }
                }
            }
            
            // 実行中リストをソート
            if (executingDirty) {
                Sort(_executingCommands);
                executingDirty = false;
            }

            // 実行中コマンドの更新
            for (var i = _executingCommands.Count - 1; i >= 0; i--) {
                var command = _executingCommands[i];
                
                // 実行継続か
                if (command.CurrentState == CommandState.Executing && command.Update()) {
                    continue;
                }
                
                // 終了
                command.Finish();
                command.Destroy();
                _executingCommands.RemoveAt(i);
                RemovedCommandEvent?.Invoke(command);
            }
        }

        /// <summary>
        /// コマンドリストの優先度ソート
        /// </summary>
        private void Sort(List<ICommand> commands) {
            commands.Sort((lhs, rhs) => lhs.Priority - rhs.Priority);
        }
    }
}