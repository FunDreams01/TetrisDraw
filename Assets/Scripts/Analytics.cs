 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using Facebook.Unity;
using System;
using ElephantSDK;
public class Analytics
{
    static int last_level = -1;
    public static void LogLevelStarted(int level)
    {
        if(last_level == -1) GameAnalytics.Initialize();
        last_level = level;
        Debug.Log("Logging Level Start: " + level);
        if(Application.isEditor) {Debug.LogWarning("Analytics will not log in Editor"); return;}
        Elephant.LevelStarted(level);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, level.ToString());
        var facebookParams = new Dictionary<string, object>();
        facebookParams[AppEventParameterName.ContentID] = "LevelStarted";
        facebookParams[AppEventParameterName.Description] = "User has loaded the level.";
        facebookParams[AppEventParameterName.Success] = "1";
        facebookParams[AppEventParameterName.Level] = level;

        FB.LogAppEvent(
            "StartedLevel",
            parameters: facebookParams
        );

    }
  
    public static void LogLevelFailed()
    {
        

        if(last_level == -1) {Debug.LogError("Called LevelFailed without starting it."); return;}
        Debug.Log("Logging Level Fail: " + last_level);
        if(Application.isEditor) {Debug.LogWarning("Analytics will not log in Editor");last_level= -1; return;}
        Elephant.LevelFailed(last_level);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, last_level.ToString());
        var facebookParams = new Dictionary<string, object>();
        facebookParams[AppEventParameterName.ContentID] = "Level Failed";
        facebookParams[AppEventParameterName.Description] = "User has failed the level.";
        facebookParams[AppEventParameterName.Success] = "1";
        facebookParams[AppEventParameterName.Level] = last_level;

        FB.LogAppEvent(
            "FailedLevel",
            parameters: facebookParams
        );

        last_level = -1;
    }

    public static void LogLevelSucceeded()
    {
        if(last_level == -1) {Debug.LogError("Called LevelSucceeded without starting it."); return;}
        Debug.Log("Logging Level Success: " + last_level);
        if(Application.isEditor) {Debug.LogWarning("Analytics will not log in Editor.");last_level= -1; return;}
        Elephant.LevelCompleted(last_level);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, last_level.ToString());
        var facebookParams = new Dictionary<string, object>();
        facebookParams[AppEventParameterName.ContentID] = "Level Succeeded";
        facebookParams[AppEventParameterName.Description] = "User has completed the level.";
        facebookParams[AppEventParameterName.Success] = "1";
        facebookParams[AppEventParameterName.Level] = last_level;

        FB.LogAppEvent(
            "SucceededLevel",
            parameters: facebookParams
        );

        last_level =-1;
    }

}  