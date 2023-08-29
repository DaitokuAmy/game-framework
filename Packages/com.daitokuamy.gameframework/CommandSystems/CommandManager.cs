using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CommandSystems {
    /// <summary>
    /// コマンド管理クラス
    /// </summary>
    public class CommandManager : IDisposable {
        private List<ICommand> _standbyCommands = new();
        private ICommand _executingCommand;

        private int _maxStandbyCount;
        private bool _standbyDirty;
        private List<int> _nextCommandIndices = new();

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
            Clear();
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

            if (command.CurrentState != CommandState.Invalid) {
                Debug.LogWarning($"invalid command. [{command.GetType()}]");
                return CommandHandle.Empty;
            }

            // 待機上限に入っていた場合、優先度が低い物を除外
            _standbyCommands.Add(command);
            command.Initialize();
            
            if (_maxStandbyCount >= 0 && _standbyCommands.Count > _maxStandbyCount) {
                _standbyCommands.Sort((lhs, rhs) => lhs.Priority - rhs.Priority);
                while (_standbyCommands.Count > _maxStandbyCount) {
                    var removeCommand = _standbyCommands[0];
                    removeCommand.Destroy();
                    _standbyCommands.RemoveAt(0);
                }
            }
            else {
                _standbyDirty = true;
            }

            return new CommandHandle(command);
        }

        /// <summary>
        /// 実行中コマンドのクリア
        /// </summary>
        public void Clear() {
            // 実行中のコマンドを強制終了
            if (_executingCommand != null) {
                _executingCommand.Destroy();
                _executingCommand = null;
            }
            
            // 実行待機中のコマンドを強制終了
            for (var i = _standbyCommands.Count - 1; i >= 0; i--) {
                _standbyCommands[i].Destroy();
                _standbyCommands.RemoveAt(i);
            }

            _standbyDirty = false;
            _nextCommandIndices.Clear();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            if (_standbyDirty) {
                _standbyCommands.Sort((lhs, rhs) => lhs.Priority - rhs.Priority);
                _standbyDirty = false;
            }

            // 実行中のCommandに関する処理
            var blockPriority = -1;
            if (_executingCommand != null) {
                // 終わっていたら廃棄
                if (_executingCommand.CurrentState != CommandState.Executing) {
                    _executingCommand.Destroy();
                    _executingCommand = null;
                }
                // 存在していたらブロック優先度を設定
                else {
                    blockPriority = _executingCommand.Priority;
                }
            }

            // 待機中コマンドを実行状態に移す
            _nextCommandIndices.Clear();
            for (var i = _standbyCommands.Count - 1; i >= 0; i--) {
                var command = _standbyCommands[i];

                // 実行状態に移せるか
                if (command.Priority > blockPriority) {
                    // 割り込み可能なら割り込む
                    if (_executingCommand == null || command.Interrupt) {
                        _nextCommandIndices.Add(i);
                    }
                }
                // Priorityが低い物は見ない
                else {
                    break;
                }
            }

            // コマンドの実行開始
            if (_nextCommandIndices.Count > 0) {
                // 実行中の物があれば廃棄
                if (_executingCommand != null) {
                    _executingCommand.Destroy();
                    _executingCommand = null;
                }
                
                // 優先度の高い物で実行可能な物を動かす
                for (var i = 0; i < _nextCommandIndices.Count; i++) {
                    var index = _nextCommandIndices[i];
                    var command = _standbyCommands[index];
                    if (command.Start()) {
                        _executingCommand = command;
                        _standbyCommands.RemoveAt(index);
                        break;
                    }
                }
                
                _nextCommandIndices.Clear();
            }

            // 実行中コマンドの更新
            if (_executingCommand != null) {
                if (!_executingCommand.Update()) {
                    // 更新が終わったら終了
                    _executingCommand.Finish();
                    _executingCommand.Destroy();
                    _executingCommand = null;
                }
            }
        }
    }
}