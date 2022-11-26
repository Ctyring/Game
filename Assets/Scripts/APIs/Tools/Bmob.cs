using UnityEngine;
using cn.bmob.api;
using cn.bmob.io;
using cn.bmob.tools;
using GlobalData;
using Model;

namespace Bomb
{
    public class Bmob : UnitySingleton<Bmob>
    {
        public static BmobUnity bmob;
        public override void Awake() {
            base.Awake();
            BmobDebug.Register (print);
            BmobDebug.level = BmobDebug.Level.TRACE;
            bmob = GameObject.Find("Canvas").AddComponent<BmobUnity>();
            bmob.ApplicationId = "9a5c8b634da9b730088076fd4f4edbf6";
            bmob.RestKey = "76d6f2e41c87e34aad26b4e5dc69e848";
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="email">邮箱</param>
        public void SignUp(string username, string password, string email)
        {
            GlobalSettings.state = GlobalSettings.States.Loading;
            GameUser user = new GameUser();
            user.username = username;
            user.password = password;
            user.email = email;
            bmob.Signup(user, (resp, exception) => 
            {
                if (resp.username == null)
                {
                    GlobalSettings.state = GlobalSettings.States.Error;
                    Debug.Log("注册失败");
                    return;
                }

                GlobalSettings.state = GlobalSettings.States.Normal;
                Debug.Log("注册成功");
            });
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public void SignIn(string username, string password)
        {
            GlobalSettings.state = GlobalSettings.States.Loading;
            
            bmob.Login<GameUser>(username, password, (resp, exception) => 
            {
                if (exception != null)
                {
                    GlobalSettings.state = GlobalSettings.States.Error;
                    print("登录失败, 失败原因为： " + exception.Message);
                    return;
                }
                GlobalSettings.state = GlobalSettings.States.Normal;
                print("登录成功, @" + resp.username + "$[" + resp.sessionToken + "]");
                print("登录成功, 当前用户对象Session： " + BmobUser.CurrentUser.sessionToken);
            });
        }
    }
}