using System;
using SampleGame.Domain.User;

namespace SampleGame.Application {
    /// <summary>
    /// UserPlayer用のアプリケーションサービス
    /// </summary>
    public class UserPlayerAppService : IDisposable {
        private UserPlayerModel _userPlayerModel;

        /// <summary>参照用のUserPlayerModel</summary>
        public IReadOnlyUserPlayerModel UserPlayerModel => _userPlayerModel;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UserPlayerAppService(UserPlayerModel userPlayerModel) {
            _userPlayerModel = userPlayerModel;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
        }
    }
}
