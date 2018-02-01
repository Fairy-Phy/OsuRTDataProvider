﻿using System;
using System.IO;
using static OsuRTDataProvider.DefaultLanguage;

namespace OsuRTDataProvider.BeatmapInfo
{
    public class Beatmap
    {
        public int BeatmapID { get; private set; }

        public string DownloadLink
        {
            get
            {
                if (BeatmapID != 0) return $"http://osu.ppy.sh/b/{BeatmapID}";
                return LANG_BEATMAP_NOT_FOUND;
            }
        }

        /// <summary>
        /// If BeatmapSetID > 0. Return beatmap's download link.
        /// </summary>
        public string DownloadLinkSet
        {
            get
            {
                if (BeatmapSetID > 0) return $"http://osu.ppy.sh/s/{BeatmapSetID}";
                return LANG_BEATMAP_NOT_FOUND;
            }
        }

        private int m_beatmap_id = -1;

        /// <summary>
        /// Return set id.
        /// If no found return -1;
        /// </summary>
        public int BeatmapSetID
        {
            get
            {
                if (m_beatmap_id > 0) return m_beatmap_id;

                if (Folder.Length > 0)
                {
                    string name = Folder;
                    int len = name.IndexOf(' ');
                    if (len != -1)
                    {
                        string id = name.Substring(0, len);

                        if (int.TryParse(id, out m_beatmap_id))
                            return m_beatmap_id;
                    }
                }
                return -1;
            }
            private set => m_beatmap_id = value;
        }

        public string Difficulty { get; private set; } = string.Empty;
        public string Creator { get; private set; } = string.Empty;
        public string Artist { get; private set; } = string.Empty;
        public string ArtistUnicode { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string TitleUnicode { get; private set; } = string.Empty;

        /// <summary>
        /// Return the first of all possible beatmap set paths.
        /// If not found.return string.Empty.
        /// </summary>
        public string Folder { get; private set; } = string.Empty;

        public int OsuClientID { get; private set; }
        public string Filename { get; private set; } = string.Empty;
        public string FilenameFull { get; private set; } = string.Empty;

        [Obsolete("LocationFile is obsoleted,Please use FilenameFull", true)]
        public string LocationFile => FilenameFull;

        public static Beatmap Empty => new Beatmap(0, -1, -1, null);

        public Beatmap(int osu_id, int set_id, int id, FileStream fs)
        {
            BeatmapSetID = set_id;
            OsuClientID = osu_id;
            BeatmapID = id;

            if (fs != null)
            {
                Folder = Path.GetDirectoryName(fs.Name);
                Filename = Path.GetFileName(fs.Name);
                FilenameFull = fs.Name;

                using (var sr = new StreamReader(fs))
                {
                    do
                    {
                        string str = sr.ReadLine().Trim();
                        if (str.StartsWith("[Metadata]"))
                        {
                            while (!sr.EndOfStream)
                            {
                                str = sr.ReadLine().Trim();
                                if (str.StartsWith("ArtistUnicode"))
                                {
                                    GetPropertyValue(str, out string val);
                                    ArtistUnicode = val;
                                }
                                else if (str.StartsWith("Artist"))
                                {
                                    GetPropertyValue(str, out string val);
                                    Artist = val;
                                }
                                else if (str.StartsWith("TitleUnicode"))
                                {
                                    GetPropertyValue(str, out string val);
                                    TitleUnicode = val;
                                }
                                else if (str.StartsWith("Title"))
                                {
                                    GetPropertyValue(str, out string val);
                                    Title = val;
                                }
                                else if (str.StartsWith("Version"))
                                {
                                    GetPropertyValue(str, out string val);
                                    Difficulty = val;
                                }
                                else if (str.StartsWith("Creator"))
                                {
                                    GetPropertyValue(str, out string val);
                                    Creator = val;
                                }
                                else if (str.StartsWith("[") || string.IsNullOrWhiteSpace(str))
                                    goto end;
                            }
                        }
                    } while (!sr.EndOfStream);
                }
            }
            end:;
        }

        public static bool operator==(Beatmap a,Beatmap b)
        {
            bool ret = false;
            ret = a.BeatmapID == b.BeatmapID;
            ret = ret && (a.BeatmapSetID == b.BeatmapSetID);
            return ret;
        }

        public static bool operator !=(Beatmap a, Beatmap b)
        {
            return !(a == b);
        }

        private static void GetPropertyValue(string line, out string val)
        {
            int pos = line.IndexOf(':');
            val = line.Substring(pos + 1).Trim();
        }
    }
}