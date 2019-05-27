using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{

    /// <summary>
    /// 普通扩展
    /// </summary>
    public static class GeneralExtensions
    {

        /// <summary>
        /// 遍历列表并进行操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcs"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> Each<T>(this IEnumerable<T> srcs, Action<T> action)
        {
            if (srcs == null) return new T[0];
            foreach (var t in srcs)
            {
                action(t);
            }
            return srcs.ToList();
        }


        /// <summary>
        /// 遍历列表并进行操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcs"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> EachwithException<T>(this IEnumerable<T> srcs, Task action, Exception ex)
        {
            foreach (var t in srcs)
            {
                action.Wait();
            }
            return srcs.ToList();
        }


        /// <summary>
        /// 是否为默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool IsDefault<T>(this T src)
        {
            var df = default(T);
            if (df == null)
            {
                return src == null;
            }
            return default(T).Equals(src);
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <typeparam name="SrcT"></typeparam>
        /// <typeparam name="dstT"></typeparam>
        /// <param name="src"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static dstT To<SrcT, dstT>(this SrcT src, Func<SrcT, dstT> act)
        {
            return act(src);
        }


        /// <summary>
        /// 转换成可空类型 通常用于 比如:string.To_nullable(x => Guid.Parse(x))
        /// </summary>
        /// <typeparam name="SrcT"></typeparam>
        /// <typeparam name="dstT"></typeparam>
        /// <param name="src"></param>
        /// <param name="act">当SrcT的src对象 不是null时，可以转换成dst的对象 的表达式</param>
        /// <returns></returns>
        public static dstT? To_nullable<SrcT, dstT>(this SrcT src, Func<SrcT, dstT> act) where SrcT : class where dstT : struct
        {
            if (src is string)
            {
                if (string.IsNullOrWhiteSpace(src as string))
                    return null;
            }
            return src != null ? (dstT?)act(src) : null;
        }


        /// <summary>
        /// 转换为 当前时区的Unspecified时间格式
        /// </summary>
        /// <param name="time"></param>
        /// <returns>当前时区的时间</returns>
        public static DateTime ToUnspecified(this DateTime time)
        {
            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local), DateTimeKind.Unspecified);
        }

        /// <summary>
        /// 将源IEnumerable<SrcT>转换为目标List
        /// </summary>
        /// <typeparam name="SrcT"></typeparam>
        /// <typeparam name="DesT">目标类型</typeparam>
        /// <param name="srcts"></param>
        /// <param name="convertAction"></param>
        /// <param name="excudeInvalid"></param>
        /// <returns></returns>
        public static List<DesT> ToList<SrcT, DesT>(this IEnumerable<SrcT> srcts, Converter<SrcT, DesT> convertAction, bool excudeInvalid = false)
        {
            var dest_list = new List<DesT>();
            foreach (var srct in srcts)
            {
                var rv = convertAction(srct);
                if (excudeInvalid && rv == null)
                {
                    continue;
                }
                dest_list.Add(rv);
            }
            return dest_list;
        }


    }
}
