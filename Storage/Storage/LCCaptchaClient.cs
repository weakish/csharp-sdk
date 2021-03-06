﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeanCloud.Storage {
    /// <summary>
    /// 验证码
    /// </summary>
    public class LCCapture {
        public string Url {
            get; set;
        }

        public string Token {
            get; set;
        }
    }

    /// <summary>
    /// 验证码工具类
    /// </summary>
    public static class LCCaptchaClient {
        /// <summary>
        /// 请求验证码
        /// </summary>
        /// <param name="width">验证码图片宽度</param>
        /// <param name="height">验证码图片高度</param>
        /// <returns></returns>
        public static async Task<LCCapture> RequestCaptcha(int width = 82,
            int height = 39) {
            string path = "requestCaptcha";
            Dictionary<string, object> queryParams = new Dictionary<string, object> {
                { "width", width },
                { "height", height }
            };
            Dictionary<string, object> response = await LCApplication.HttpClient.Get<Dictionary<string, object>>(path, queryParams: queryParams);
            return new LCCapture {
                Url = response["captcha_url"] as string,
                Token = response["captcha_token"] as string
            };
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="code"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task VerifyCaptcha(string code,
            string token) {
            if (string.IsNullOrEmpty(code)) {
                throw new ArgumentNullException(nameof(code));
            }
            if (string.IsNullOrEmpty(token)) {
                throw new ArgumentNullException(nameof(token));
            }

            string path = "verifyCaptcha";
            Dictionary<string, object> data = new Dictionary<string, object> {
                { "captcha_code", code },
                { "captcha_token", token }
            };
            await LCApplication.HttpClient.Post<Dictionary<string, object>>(path, data: data);
        }
    }
}
