﻿using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloud {
    /// <summary>
    /// AVFile 存储于 LeanCloud 的文件类
    /// </summary>
    [AVClassName("_File")]
    public class AVFile : AVObject {
        const string QCloud = "qcloud";
        const string AWS = "s3";

        #region Properties

        /// <summary>
        /// Gets the name of the file. Before save is called, this is the filename given by
        /// the user. After save is called, that name gets prefixed with a unique identifier.
        /// </summary>
        [AVFieldName("name")]
        public string Name {
            get {
                return GetProperty<string>("Name");
            }
            set {
                SetProperty(value, "Name");
            }
        }

        [AVFieldName("key")]
        public string Key {
            get {
                return GetProperty<string>("Key");
            }
            set {
                SetProperty(value, "Key");
            }
        }

        /// <summary>
        /// Gets the MIME type of the file. This is either passed in to the constructor or
        /// inferred from the file extension. "unknown/unknown" will be used if neither is 
        /// available.
        /// </summary>
        [AVFieldName("mime_type")]
        public string MimeType {
            get {
                return GetProperty<string>("MimeType");
            }
            set {
                SetProperty(value, "MimeType");
            }
        }

        /// <summary>
        /// Gets the url of the file. It is only available after you save the file or after
        /// you get the file from a <see cref="AVObject"/>.
        /// </summary>
        [AVFieldName("url")]
        public string Url {
            get {
                return GetProperty<string>("Url");
            }
            set {
                SetProperty(value, "Url");
            }
        }

        /// <summary>
        /// 文件的元数据。
        /// </summary>
        [AVFieldName("metaData")]
        public IDictionary<string, object> MetaData {
            get {
                return GetProperty<IDictionary<string, object>>("MetaData");
            }
            set {
                SetProperty(value, "MetaData");
            }
        }

        #endregion

        private readonly Stream dataStream;

        #region Constructor
        /// <summary>
        ///  通过文件名，数据流，文件类型，文件源信息构建一个 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">数据流</param>
        /// <param name="mimeType">文件类型</param>
        /// <param name="metaData">文件源信息</param>
        public AVFile(string name, Stream data, string mimeType = null, IDictionary<string, object> metaData = null) {
            Name = name;
            MimeType = mimeType ?? GetMIMEType(name);
            MetaData = metaData;
            dataStream = data;
        }

        /// <summary>
        /// 根据文件名，文件 Byte 数组，以及文件类型构建 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件 Byte 数组</param>
        /// <param name="mimeType">文件类型</param>
        public AVFile(string name, byte[] data, string mimeType = null)
            : this(name, new MemoryStream(data), mimeType) { }

        /// <summary>
        /// 根据文件名，文件流数据，文件类型构建 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件流数据</param>
        /// <param name="mimeType">文件类型</param>
        public AVFile(string name, Stream data, string mimeType = null)
            : this(name, data, mimeType, new Dictionary<string, object>()) {
        }

        /// <summary>
        /// 根据 byte 数组以及文件名创建文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件的 byte[] 数据</param>
        public AVFile(string name, byte[] data)
            : this(name, new MemoryStream(data), new Dictionary<string, object>()) {

        }

        /// <summary>
        /// 根据文件名，数据 byte[] 数组以及元数据创建文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件的 byte[] 数据</param>
        /// <param name="metaData">元数据</param>
        public AVFile(string name, byte[] data, IDictionary<string, object> metaData)
            : this(name, new MemoryStream(data), metaData) {

        }

        /// <summary>
        /// 根据文件名，数据流以及元数据创建文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件的数据流</param>
        /// <param name="metaData">元数据</param>
        public AVFile(string name, Stream data, IDictionary<string, object> metaData)
            : this(name, data, GetMIMEType(name), metaData) {
        }

        /// <summary>
        /// 根据文件名，数据流以及元数据创建文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="data">文件的数据流</param>
        public AVFile(string name, Stream data)
            : this(name, data, new Dictionary<string, object>()) {

        }

        public AVFile() {

        }

        #endregion

        #region created by url or uri

        /// <summary>
        /// 根据文件名，Uri，文件类型以及文件源信息
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="uri">文件Uri</param>
        /// <param name="mimeType">文件类型</param>
        /// <param name="metaData">文件源信息</param>
        public AVFile(string name, Uri uri, string mimeType = null, IDictionary<string, object> metaData = null) {
            Name = name;
            MimeType = mimeType ?? GetMIMEType(name);
            IsExternal = true;
        }

        /// <summary>
        /// 根据文件名，文件 Url，文件类型，文件源信息构建 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="url">文件 Url</param>
        /// <param name="mimeType">文件类型</param>
        /// <param name="metaData">文件源信息</param>
        public AVFile(string name, string url, string mimeType = null, IDictionary<string, object> metaData = null)
            : this(name, new Uri(url), mimeType, metaData) {

        }

        /// <summary>
        /// 根据文件名，文件 Url以及文件的源信息构建 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="url">文件 Url</param>
        /// <param name="metaData">文件源信息</param>
        public AVFile(string name, string url, IDictionary<string, object> metaData)
            : this(name, url, null, metaData) {
        }

        /// <summary>
        /// 根据文件名，文件 Uri，以及文件类型构建 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="uri">文件 Uri</param>
        /// <param name="mimeType">文件类型</param>
        public AVFile(string name, Uri uri, string mimeType = null)
            : this(name, uri, mimeType, new Dictionary<string, object>()) {

        }

        /// <summary>
        /// 根据文件名以及文件 Uri 构建 AVFile
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="uri">文件 Uri</param>
        public AVFile(string name, Uri uri)
            : this(name, uri, null, new Dictionary<string, object>()) {

        }
        /// <summary>
        /// 根据文件名和 Url 创建文件
        /// </summary> 
        /// <param name="name">文件名</param>
        /// <param name="url">文件的 Url</param>
        public AVFile(string name, string url) {
            Name = name;
            Url = url;
            IsExternal = true;
        }

        #endregion

        

        /// <summary>
        /// 获取缩略图
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="quality">质量 1-100</param>
        /// <param name="scaleToFit">等比缩放，是否裁剪</param>
        /// <param name="format">格式</param>
        /// <returns></returns>
        public string GetThumbnailUrl(int width, int height,
            int quality = 100, bool scaleToFit = true, string format = "png") {
            int mode = scaleToFit ? 2 : 1;
            return $"{Url}?imageView/{mode}/w/{width}/h/{height}/q/{quality}/format/{format}";
        }

        internal static AVFileController FileController {
            get {
                return AVPlugins.Instance.FileController;
            }
        }

        IDictionary<string, object> ToJSON() {
            if (IsDirty) {
                throw new InvalidOperationException(
                  "AVFile must be saved before it can be serialized.");
            }
            return new Dictionary<string, object> {
                { "__type", "File"} ,
                { "id", ObjectId },
                { "name", Name },
                { "url", Url }
            };
        }

        #region Save

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public override async Task SaveAsync(bool fetchWhenSave = false, AVQuery<AVObject> query = null, CancellationToken cancellationToken = default) {
            if (IsExternal) {
                await base.SaveAsync(fetchWhenSave, query, cancellationToken);
            } else {
                // 需不需要判断数据的 dirty?
                // 获取 FileToken 并创建对应记录
                Tuple<HttpStatusCode, IDictionary<string, object>> res = await AVPlugins.Instance.FileController.GetFileToken(Name, MetaData);
                IObjectState serverState = AVObjectCoder.Instance.Decode(res.Item2, AVDecoder.Instance);
                HandleSave(serverState);
                IDictionary<string, object> fileData = res.Item2;
                AVClient.PrintLog(fileData.ToString());

                var provider = fileData["provider"] as string;
                switch (provider) {
                    case QCloud: {
                            await new QCloudUploader {
                                FileName = Key,
                                UploadUrl = fileData["upload_url"] as string,
                                Token = fileData["token"] as string,
                                Bucket = fileData["bucket"] as string,
                                Stream = dataStream
                            }.Upload();
                        }
                        break;
                    case AWS: {
                            await new AWSUploader {
                                UploadUrl = fileData["upload_url"] as string,
                                MimeType = MimeType,
                                Stream = dataStream
                            }.Upload();
                        }
                        break;
                    default: {
                            await new QiniuUploader {
                                Key = Key,
                                Token = fileData["token"] as string,
                                MimeType = MimeType,
                                MetaData = MetaData,
                                Stream = dataStream
                            }.Upload();
                        }
                        break;
                }
            }
        }

        #endregion

        #region Compatible

        /// <summary>
        /// 文件是否为外链文件。
        /// </summary>
        /// <value>
        /// </value>
        public bool IsExternal {
            get; private set;
        }

        internal static string GetUniqueName(string name) {
            string key = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(name);
            key += extension;
            return key;
        }

        internal static string GetMIMEType(string fileName) {
            try {
                string str = Path.GetExtension(fileName).Remove(0, 1);
                if (!MIMETypesDictionary.ContainsKey(str)) {
                    return "unknown/unknown";
                }
                return MIMETypesDictionary[str];
            } catch {
                return "unknown/unknown";
            }
        }

        //public static AVFile CreateWithData(string objectId, string name, string url, IDictionary<string, object> metaData) {
        //    var fileState = new FileState {
        //        Name = name,
        //        ObjectId = objectId,
        //        Url = new Uri(url),
        //        MetaData = metaData
        //    };
        //    return CreateWithState(fileState);
        //}
        
        #endregion

        private readonly static Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string> {
            { "ai", "application/postscript" },
            { "aif", "audio/x-aiff" },
            { "aifc", "audio/x-aiff" },
            { "aiff", "audio/x-aiff" },
            { "asc", "text/plain" },
            { "atom", "application/atom+xml" },
            { "au", "audio/basic" },
            { "avi", "video/x-msvideo" },
            { "bcpio", "application/x-bcpio" },
            { "bin", "application/octet-stream" },
            { "bmp", "image/bmp" },
            { "cdf", "application/x-netcdf" },
            { "cgm", "image/cgm" },
            { "class", "application/octet-stream" },
            { "cpio", "application/x-cpio" },
            { "cpt", "application/mac-compactpro" },
            { "csh", "application/x-csh" },
            { "css", "text/css" },
            { "dcr", "application/x-director" },
            { "dif", "video/x-dv" },
            { "dir", "application/x-director" },
            { "djv", "image/vnd.djvu" },
            { "djvu", "image/vnd.djvu" },
            { "dll", "application/octet-stream" },
            { "dmg", "application/octet-stream" },
            { "dms", "application/octet-stream" },
            { "doc", "application/msword" },
            { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { "dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
            { "docm", "application/vnd.ms-word.document.macroEnabled.12" },
            { "dotm", "application/vnd.ms-word.template.macroEnabled.12" },
            { "dtd", "application/xml-dtd" },
            { "dv", "video/x-dv" },
            { "dvi", "application/x-dvi" },
            { "dxr", "application/x-director" },
            { "eps", "application/postscript" },
            { "etx", "text/x-setext" },
            { "exe", "application/octet-stream" },
            { "ez", "application/andrew-inset" },
            { "gif", "image/gif" },
            { "gram", "application/srgs" },
            { "grxml", "application/srgs+xml" },
            { "gtar", "application/x-gtar" },
            { "hdf", "application/x-hdf" },
            { "hqx", "application/mac-binhex40" },
            { "htm", "text/html" },
            { "html", "text/html" },
            { "ice", "x-conference/x-cooltalk" },
            { "ico", "image/x-icon" },
            { "ics", "text/calendar" },
            { "ief", "image/ief" },
            { "ifb", "text/calendar" },
            { "iges", "model/iges" },
            { "igs", "model/iges" },
            { "jnlp", "application/x-java-jnlp-file" },
            { "jp2", "image/jp2" },
            { "jpe", "image/jpeg" },
            { "jpeg", "image/jpeg" },
            { "jpg", "image/jpeg" },
            { "js", "application/x-javascript" },
            { "kar", "audio/midi" },
            { "latex", "application/x-latex" },
            { "lha", "application/octet-stream" },
            { "lzh", "application/octet-stream" },
            { "m3u", "audio/x-mpegurl" },
            { "m4a", "audio/mp4a-latm" },
            { "m4b", "audio/mp4a-latm" },
            { "m4p", "audio/mp4a-latm" },
            { "m4u", "video/vnd.mpegurl" },
            { "m4v", "video/x-m4v" },
            { "mac", "image/x-macpaint" },
            { "man", "application/x-troff-man" },
            { "mathml", "application/mathml+xml" },
            { "me", "application/x-troff-me" },
            { "mesh", "model/mesh" },
            { "mid", "audio/midi" },
            { "midi", "audio/midi" },
            { "mif", "application/vnd.mif" },
            { "mov", "video/quicktime" },
            { "movie", "video/x-sgi-movie" },
            { "mp2", "audio/mpeg" },
            { "mp3", "audio/mpeg" },
            { "mp4", "video/mp4" },
            { "mpe", "video/mpeg" },
            { "mpeg", "video/mpeg" },
            { "mpg", "video/mpeg" },
            { "mpga", "audio/mpeg" },
            { "ms", "application/x-troff-ms" },
            { "msh", "model/mesh" },
            { "mxu", "video/vnd.mpegurl" },
            { "nc", "application/x-netcdf" },
            { "oda", "application/oda" },
            { "ogg", "application/ogg" },
            { "pbm", "image/x-portable-bitmap" },
            { "pct", "image/pict" },
            { "pdb", "chemical/x-pdb" },
            { "pdf", "application/pdf" },
            { "pgm", "image/x-portable-graymap" },
            { "pgn", "application/x-chess-pgn" },
            { "pic", "image/pict" },
            { "pict", "image/pict" },
            { "png", "image/png" },
            { "pnm", "image/x-portable-anymap" },
            { "pnt", "image/x-macpaint" },
            { "pntg", "image/x-macpaint" },
            { "ppm", "image/x-portable-pixmap" },
            { "ppt", "application/vnd.ms-powerpoint" },
            { "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { "potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
            { "ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
            { "ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
            { "pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
            { "potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
            { "ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
            { "ps", "application/postscript" },
            { "qt", "video/quicktime" },
            { "qti", "image/x-quicktime" },
            { "qtif", "image/x-quicktime" },
            { "ra", "audio/x-pn-realaudio" },
            { "ram", "audio/x-pn-realaudio" },
            { "ras", "image/x-cmu-raster" },
            { "rdf", "application/rdf+xml" },
            { "rgb", "image/x-rgb" },
            { "rm", "application/vnd.rn-realmedia" },
            { "roff", "application/x-troff" },
            { "rtf", "text/rtf" },
            { "rtx", "text/richtext" },
            { "sgm", "text/sgml" },
            { "sgml", "text/sgml" },
            { "sh", "application/x-sh" },
            { "shar", "application/x-shar" },
            { "silo", "model/mesh" },
            { "sit", "application/x-stuffit" },
            { "skd", "application/x-koan" },
            { "skm", "application/x-koan" },
            { "skp", "application/x-koan" },
            { "skt", "application/x-koan" },
            { "smi", "application/smil" },
            { "smil", "application/smil" },
            { "snd", "audio/basic" },
            { "so", "application/octet-stream" },
            { "spl", "application/x-futuresplash" },
            { "src", "application/x-wais-Source" },
            { "sv4cpio", "application/x-sv4cpio" },
            { "sv4crc", "application/x-sv4crc" },
            { "svg", "image/svg+xml" },
            { "swf", "application/x-shockwave-flash" },
            { "t", "application/x-troff" },
            { "tar", "application/x-tar" },
            { "tcl", "application/x-tcl" },
            { "tex", "application/x-tex" },
            { "texi", "application/x-texinfo" },
            { "texinfo", "application/x-texinfo" },
            { "tif", "image/tiff" },
            { "tiff", "image/tiff" },
            { "tr", "application/x-troff" },
            { "tsv", "text/tab-separated-values" },
            { "txt", "text/plain" },
            { "ustar", "application/x-ustar" },
            { "vcd", "application/x-cdlink" },
            { "vrml", "model/vrml" },
            { "vxml", "application/voicexml+xml" },
            { "wav", "audio/x-wav" },
            { "wbmp", "image/vnd.wap.wbmp" },
            { "wbmxl", "application/vnd.wap.wbxml" },
            { "wml", "text/vnd.wap.wml" },
            { "wmlc", "application/vnd.wap.wmlc" },
            { "wmls", "text/vnd.wap.wmlscript" },
            { "wmlsc", "application/vnd.wap.wmlscriptc" },
            { "wrl", "model/vrml" },
            { "xbm", "image/x-xbitmap" },
            { "xht", "application/xhtml+xml" },
            { "xhtml", "application/xhtml+xml" },
            { "xls", "application/vnd.ms-excel" },
            { "xml", "application/xml" },
            { "xpm", "image/x-xpixmap" },
            { "xsl", "application/xml" },
            { "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { "xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
            { "xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
            { "xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
            { "xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
            { "xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
            { "xslt", "application/xslt+xml" },
            { "xul", "application/vnd.mozilla.xul+xml" },
            { "xwd", "image/x-xwindowdump" },
            { "xyz", "chemical/x-xyz" },
            { "zip", "application/zip" }
        };
    }
}
