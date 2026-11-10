using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicAutoPlayer
{
    /// <summary>
    /// 日程时间计算：日程只保存“每天的时刻”，每次计时都投影到当天，
    /// 避免把程序启动当天的日期写死后跨天失效（倒计时变负、任务不触发）。
    /// </summary>
    public static class ScheduleMath
    {
        /// <summary>
        /// 返回 >= now 的最近一次任务；今天已经全部结束时返回明天最早的一次。
        /// 返回值与 now 同一天表示“今天还有任务”，否则表示“今天已无任务”。
        /// </summary>
        public static DateTime? NextOccurrence(IReadOnlyList<TimeSpan> timesOfDay, DateTime now)
        {
            if (timesOfDay.Count == 0)
                return null;

            DateTime today = now.Date;
            DateTime? next = null;
            foreach (TimeSpan t in timesOfDay)
            {
                DateTime occurrence = today + t;
                if (occurrence >= now && (next == null || occurrence < next.Value))
                    next = occurrence;
            }

            return next ?? today.AddDays(1) + timesOfDay.Min();
        }

        /// <summary>
        /// 找出 (prev, now] 内、且迟到不超过 maxLag 的最近一次任务。
        /// 用于补发因计时器卡顿（UI 阻塞、短暂休眠）而跨过零点的播放。
        /// </summary>
        public static DateTime? LatestDue(IReadOnlyList<TimeSpan> timesOfDay, DateTime prev, DateTime now, TimeSpan maxLag)
        {
            if (timesOfDay.Count == 0 || now <= prev)
                return null;

            DateTime? latest = null;
            // 只需看昨天和今天，跨零点的那一次也覆盖得到
            for (DateTime day = now.Date.AddDays(-1); day <= now.Date; day = day.AddDays(1))
            {
                foreach (TimeSpan t in timesOfDay)
                {
                    DateTime occurrence = day + t;
                    if (occurrence <= prev || occurrence > now || now - occurrence > maxLag)
                        continue;
                    if (latest == null || occurrence > latest.Value)
                        latest = occurrence;
                }
            }

            return latest;
        }

        /// <summary>倒计时文本（时:分:秒）；差值不会为负。</summary>
        public static string Format(TimeSpan diff)
        {
            if (diff < TimeSpan.Zero)
                diff = TimeSpan.Zero;

            return $"{(int)diff.TotalHours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
        }
    }
}
