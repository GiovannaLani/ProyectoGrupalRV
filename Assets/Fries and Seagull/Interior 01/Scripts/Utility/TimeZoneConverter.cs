﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seagull.Interior_01.Utility {
    public static class TimeZoneConverter {
        private static readonly Dictionary<TimeZones, string> timeZoneMap = new Dictionary<TimeZones, string>() {
            { TimeZones.DatelineStandardTime, "Dateline Standard Time" },
            { TimeZones.UTCC11, "UTC-11" },
            { TimeZones.HawaiianStandardTime, "Hawaiian Standard Time" },
            { TimeZones.AleutianStandardTime, "Aleutian Standard Time" },
            { TimeZones.MarquesasStandardTime, "Marquesas Standard Time" },
            { TimeZones.UTC09, "UTC-09" },
            { TimeZones.AlaskanStandardTime, "Alaskan Standard Time" },
            { TimeZones.PacificStandardTimeMexico, "Pacific Standard Time (Mexico)" },
            { TimeZones.UTC08, "UTC-08" },
            { TimeZones.PacificStandardTime, "Pacific Standard Time" },
            { TimeZones.USMountainStandardTime, "US Mountain Standard Time" },
            { TimeZones.MountainStandardTime, "Mountain Standard Time" },
            { TimeZones.MountainStandardTimeMexico, "Mountain Standard Time (Mexico)" },
            { TimeZones.YukonStandardTime, "Yukon Standard Time" },
            { TimeZones.CentralAmericaStandardTime, "Central America Standard Time" },
            { TimeZones.CentralStandardTime, "Central Standard Time" },
            { TimeZones.EasterIslandStandardTime, "Easter Island Standard Time" },
            { TimeZones.CentralStandardTimeMexico, "Central Standard Time (Mexico)" },
            { TimeZones.CanadaCentralStandardTime, "Canada Central Standard Time" },
            { TimeZones.EasternStandardTime, "Eastern Standard Time" },
            { TimeZones.EasternStandardTimeMexico, "Eastern Standard Time (Mexico)" },
            { TimeZones.USEasternStandardTime, "US Eastern Standard Time" },
            { TimeZones.CubaStandardTime, "Cuba Standard Time" },
            { TimeZones.SAPacificStandardTime, "SA Pacific Standard Time" },
            { TimeZones.HaitiStandardTime, "Haiti Standard Time" },
            { TimeZones.TurksAndCaicosStandardTime, "Turks and Caicos Standard Time" },
            { TimeZones.SAWesternStandardTime, "SA Western Standard Time" },
            { TimeZones.ParaguayStandardTime, "Paraguay Standard Time" },
            { TimeZones.VenezuelaStandardTime, "Venezuela Standard Time" },
            { TimeZones.PacificSAStandardTime, "Pacific SA Standard Time" },
            { TimeZones.AtlanticStandardTime, "Atlantic Standard Time" },
            { TimeZones.CentralBrazilianStandardTime, "Central Brazilian Standard Time" },
            { TimeZones.NewfoundlandStandardTime, "Newfoundland Standard Time" },
            { TimeZones.SAEasternStandardTime, "SA Eastern Standard Time" },
            { TimeZones.SaintPierreStandardTime, "Saint Pierre Standard Time" },
            { TimeZones.ESouthAmericaStandardTime, "E. South America Standard Time" },
            { TimeZones.ArgentinaStandardTime, "Argentina Standard Time" },
            { TimeZones.BahiaStandardTime, "Bahia Standard Time" },
            { TimeZones.MontevideoStandardTime, "Montevideo Standard Time" },
            { TimeZones.MagallanesStandardTime, "Magallanes Standard Time" },
            { TimeZones.TocantinsStandardTime, "Tocantins Standard Time" },
            { TimeZones.MidAtlanticStandardTime, "Mid-Atlantic Standard Time" },
            { TimeZones.UTC02, "UTC-02" },
            { TimeZones.GreenlandStandardTime, "Greenland Standard Time" },
            { TimeZones.AzoresStandardTime, "Azores Standard Time" },
            { TimeZones.CapeVerdeStandardTime, "Cape Verde Standard Time" },
            { TimeZones.UTC, "UTC" },
            { TimeZones.SaoTomeStandardTime, "Sao Tome Standard Time" },
            { TimeZones.GreenwichStandardTime, "Greenwich Standard Time" },
            { TimeZones.GMTStandardTime, "GMT Standard Time" },
            { TimeZones.MoroccoStandardTime, "Morocco Standard Time" },
            { TimeZones.WCentralAfricaStandardTime, "W. Central Africa Standard Time" },
            { TimeZones.RomanceStandardTime, "Romance Standard Time" },
            { TimeZones.CentralEuropeanStandardTime, "Central European Standard Time" },
            { TimeZones.CentralEuropeStandardTime, "Central Europe Standard Time" },
            { TimeZones.WEuropeStandardTime, "W. Europe Standard Time" },
            { TimeZones.WestBankStandardTime, "West Bank Standard Time" },
            { TimeZones.KaliningradStandardTime, "Kaliningrad Standard Time" },
            { TimeZones.SouthAfricaStandardTime, "South Africa Standard Time" },
            { TimeZones.SudanStandardTime, "Sudan Standard Time" },
            { TimeZones.EEuropeStandardTime, "E. Europe Standard Time" },
            { TimeZones.EgyptStandardTime, "Egypt Standard Time" },
            { TimeZones.SouthSudanStandardTime, "South Sudan Standard Time" },
            { TimeZones.NamibiaStandardTime, "Namibia Standard Time" },
            { TimeZones.LibyaStandardTime, "Libya Standard Time" },
            { TimeZones.IsraelStandardTime, "Israel Standard Time" },
            { TimeZones.MiddleEastStandardTime, "Middle East Standard Time" },
            { TimeZones.FLEStandardTime, "FLE Standard Time" },
            { TimeZones.GTBStandardTime, "GTB Standard Time" },
            { TimeZones.TurkeyStandardTime, "Turkey Standard Time" },
            { TimeZones.VolgogradStandardTime, "Volgograd Standard Time" },
            { TimeZones.EAfricaStandardTime, "E. Africa Standard Time" },
            { TimeZones.SyriaStandardTime, "Syria Standard Time" },
            { TimeZones.JordanStandardTime, "Jordan Standard Time" },
            { TimeZones.ArabicStandardTime, "Arabic Standard Time" },
            { TimeZones.BelarusStandardTime, "Belarus Standard Time" },
            { TimeZones.ArabStandardTime, "Arab Standard Time" },
            { TimeZones.RussianStandardTime, "Russian Standard Time" },
            { TimeZones.IranStandardTime, "Iran Standard Time" },
            { TimeZones.RussiaTimeZone3, "Russia Time Zone 3" },
            { TimeZones.CaucasusStandardTime, "Caucasus Standard Time" },
            { TimeZones.AzerbaijanStandardTime, "Azerbaijan Standard Time" },
            { TimeZones.GeorgianStandardTime, "Georgian Standard Time" },
            { TimeZones.SaratovStandardTime, "Saratov Standard Time" },
            { TimeZones.MauritiusStandardTime, "Mauritius Standard Time" },
            { TimeZones.ArabianStandardTime, "Arabian Standard Time" },
            { TimeZones.AstrakhanStandardTime, "Astrakhan Standard Time" },
            { TimeZones.AfghanistanStandardTime, "Afghanistan Standard Time" },
            { TimeZones.PakistanStandardTime, "Pakistan Standard Time" },
            { TimeZones.EkaterinburgStandardTime, "Ekaterinburg Standard Time" },
            { TimeZones.WestAsiaStandardTime, "West Asia Standard Time" },
            { TimeZones.QyzylordaStandardTime, "Qyzylorda Standard Time" },
            { TimeZones.SriLankaStandardTime, "Sri Lanka Standard Time" },
            { TimeZones.IndiaStandardTime, "India Standard Time" },
            { TimeZones.NepalStandardTime, "Nepal Standard Time" },
            { TimeZones.CentralAsiaStandardTime, "Central Asia Standard Time" },
            { TimeZones.BangladeshStandardTime, "Bangladesh Standard Time" },
            { TimeZones.OmskStandardTime, "Omsk Standard Time" },
            { TimeZones.MyanmarStandardTime, "Myanmar Standard Time" },
            { TimeZones.NorthAsiaStandardTime, "North Asia Standard Time" },
            { TimeZones.AltaiStandardTime, "Altai Standard Time" },
            { TimeZones.TomskStandardTime, "Tomsk Standard Time" },
            { TimeZones.NCentralAsiaStandardTime, "N. Central Asia Standard Time" },
            { TimeZones.SEAsiaStandardTime, "SE Asia Standard Time" },
            { TimeZones.WMongoliaStandardTime, "W. Mongolia Standard Time" },
            { TimeZones.UlaanbaatarStandardTime, "Ulaanbaatar Standard Time" },
            { TimeZones.NorthAsiaEastStandardTime, "North Asia East Standard Time" },
            { TimeZones.ChinaStandardTime, "China Standard Time" },
            { TimeZones.TaipeiStandardTime, "Taipei Standard Time" },
            { TimeZones.SingaporeStandardTime, "Singapore Standard Time" },
            { TimeZones.WAustraliaStandardTime, "W. Australia Standard Time" },
            { TimeZones.AusCentralWStandardTime, "Aus Central W. Standard Time" },
            { TimeZones.TokyoStandardTime, "Tokyo Standard Time" },
            { TimeZones.NorthKoreaStandardTime, "North Korea Standard Time" },
            { TimeZones.TransbaikalStandardTime, "Transbaikal Standard Time" },
            { TimeZones.YakutskStandardTime, "Yakutsk Standard Time" },
            { TimeZones.KoreaStandardTime, "Korea Standard Time" },
            { TimeZones.AUSCentralStandardTime, "AUS Central Standard Time" },
            { TimeZones.CenAustraliaStandardTime, "Cen. Australia Standard Time" },
            { TimeZones.WestPacificStandardTime, "West Pacific Standard Time" },
            { TimeZones.AUSEasternStandardTime, "AUS Eastern Standard Time" },
            { TimeZones.EAustraliaStandardTime, "E. Australia Standard Time" },
            { TimeZones.VladivostokStandardTime, "Vladivostok Standard Time" },
            { TimeZones.TasmaniaStandardTime, "Tasmania Standard Time" },
            { TimeZones.LordHoweStandardTime, "Lord Howe Standard Time" },
            { TimeZones.RussiaTimeZone10, "Russia Time Zone 10" },
            { TimeZones.BougainvilleStandardTime, "Bougainville Standard Time" },
            { TimeZones.CentralPacificStandardTime, "Central Pacific Standard Time" },
            { TimeZones.SakhalinStandardTime, "Sakhalin Standard Time" },
            { TimeZones.NorfolkStandardTime, "Norfolk Standard Time" },
            { TimeZones.MagadanStandardTime, "Magadan Standard Time" },
            { TimeZones.UTC12, "UTC+12" },
            { TimeZones.NewZealandStandardTime, "New Zealand Standard Time" },
            { TimeZones.KamchatkaStandardTime, "Kamchatka Standard Time" },
            { TimeZones.FijiStandardTime, "Fiji Standard Time" },
            { TimeZones.RussiaTimeZone11, "Russia Time Zone 11" },
            { TimeZones.ChathamIslandsStandardTime, "Chatham Islands Standard Time" },
            { TimeZones.TongaStandardTime, "Tonga Standard Time" },
            { TimeZones.UTC13, "UTC+13" },
            { TimeZones.SamoaStandardTime, "Samoa Standard Time" },
            { TimeZones.LineIslandsStandardTime, "Line Islands Standard Time" }
        };

        public static TimeZoneInfo GetTimeZoneInfo(TimeZones timeZoneEnum) {
            string timeZoneId;
            if (timeZoneMap.TryGetValue(timeZoneEnum, out timeZoneId)) {
                try {
                    return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch (TimeZoneNotFoundException) {
                    Debug.LogError("The timezone was not found on the system.");
                }
                catch (InvalidTimeZoneException) {
                    Debug.LogError("The timezone data is corrupt.");
                }
            }
            else {
                Debug.LogError("The timezone enum value is not mapped to an ID.");
            }

            return null;
        }
    }
}