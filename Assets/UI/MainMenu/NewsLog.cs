using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class NewsLog : MonoBehaviour
{
    private static bool newsLoaded = false;

    private static readonly string NEWSLOG_URL = "https://raw.githubusercontent.com/yozozchomutova/StateOfWarAnnihilation_installer/main/news_log.txt";

    public MainMenuPanel moreNewsPanel;

    void Start()
    {
        newsLoaded = false;
    }

    void Update()
    {
        
    }

    public void OnReadMore()
    {
        moreNewsPanel.show();

        if (!newsLoaded)
        {
            using (WebClient client = new WebClient())
            {
                string s = client.DownloadString(NEWSLOG_URL);
                string[] lines = s.Split(
                    new string[] { Environment.NewLine },
                    StringSplitOptions.None
                );

                //Analyze text line by line
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "[")
                    {

                    }
                }

                newsLoaded = true;
            }
        }
    }

    public void OnCloseReadMore()
    {
        moreNewsPanel.OnClose();
    }
}
