/********************************************************************
    Created:    2014/09/29
    File Base:  TimerHelper.cs
    Author:     Adam

    Purpose:    Time helper class.
*********************************************************************/
using UnityEngine;
using System;
using System.Collections;

public class TimeHelper
{
    /// <summary>
    /// Delay execute an action.
    /// </summary>
    /// <param name="funcDelayed"></param>
    /// <param name="waitTime">unit as seconds</param>
    /// <returns></returns>
    public static IEnumerator DelayExecuteMethod(Action funcDelayed, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        funcDelayed.Invoke();
    }

    public static void DelayExecuteMethod(Action funcDelayed, float waitTime, MonoBehaviour target)
    {
        target.StartCoroutine(DelayExecuteMethod(funcDelayed, waitTime));
    }

    public static long ToUnixTime(DateTime date)
    {
        var epoch = new DateTime(1970, 1, 1);
        return Convert.ToInt64((date - epoch).TotalSeconds);
    }

    public static DateTime FromUnixTime(long unixTime)
    {
        var epoch = new DateTime(1970, 1, 1);
        return epoch.AddSeconds(unixTime);
    }

    /// <summary>
    /// Time since system boot as millisecond. Cycles 
    /// between zero and Int32.MaxValue once every 24.9 days.
    /// </summary>
    /// <returns>milliseconds</returns>
    public static int CurrentTimeMeasured()
    {
        return Environment.TickCount & Int32.MaxValue;
    }

    /// <summary>
    /// Return escaped time since start as millisecond.
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public static int TimeEscaped(int start)
    {
        int current = CurrentTimeMeasured();
        if (current >= start)
        {
            return current - start;
        }

        return Int32.MaxValue - start + current;
    }

    /// <summary>
    /// 获取两个日期之间间隔的天数
    /// </summary>
    /// <param name="begainDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public static int GetSubDays(DateTime begainDate, DateTime endDate)
    {
        begainDate = begainDate.AddSeconds(24 * 3600 - begainDate.TimeOfDay.TotalSeconds);
        endDate = endDate.AddSeconds(24 * 3600 - endDate.TimeOfDay.TotalSeconds);
        return (int)(endDate - begainDate).TotalDays + 1;
    }

    /// <summary>
    /// 获取两个日期之间间隔的天数
    /// </summary>
    /// <param name="begainTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public static int GetSubDays(uint begainTime, uint endTime)
    {
        DateTime begainDate = new DateTime().AddSeconds(begainTime);
        DateTime endDate = new DateTime().AddSeconds(endTime);
        return GetSubDays(begainDate, endDate);
    }

}
