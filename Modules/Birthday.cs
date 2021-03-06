﻿using System;
using System.Collections.Generic;
using Discord.Commands;
using System.Threading.Tasks;
using System.IO;
using KillianBot.Services;
using Discord;
using Discord.WebSocket;
using System.Linq;
using Discord.Net;

namespace KillianBot.Modules
{

    public class Birthday : ModuleBase<SocketCommandContext>
    {
        string filePath = System.AppContext.BaseDirectory + Config.config.BirthdayFileName;
        
        /* Assuming the birthday file is formatted thusly
         * 
         * Name,mm/dd/yyyy
         * ...
         * Example:
         * John,2/22/2000
         * ...
         * 
         * For safety sake I would suggest names not contain special character (or at the very least not commas)
         * Names should also be unique for more reasons than "Wait, which one of you is this?" (Dictionaries)
         */
        //UNTESTED
        [Discord.Commands.Command("Birthday"), Summary("Checks birthdays and sends next birthday or checks if someones birthday is today.")]
        [Discord.Commands.RequireUserPermission(GuildPermission.SendMessages)]
        [Discord.Commands.Alias("bday")]
        public async Task birthday()
        {
            string[] birthdayLines;
            Dictionary<string, DateTime> parsedBirthdays = new Dictionary<string, DateTime>();
            List<string> todayBirthdayNames = new List<string>();
            var nextClosestBirthday = ("", DateTime.Now.AddYears(1));

            try
            {
                birthdayLines = File.ReadAllLines(filePath);
            }
            catch
            {
                //TODO: Replace all the strings in the ReplyAsync methods with variables which come from a file.
                await ReplyAsync("There was a problem reading the birthday file.");
                return;
            }
            foreach (String birthdayLine in birthdayLines)
            {
                string[] birthdayChunks = birthdayLine.Split(",");
                try
                {
                    DateTime birthDate = DateTime.Parse(birthdayChunks[1]);
                    //Sets year of birthday to this year
                    birthDate.AddYears(DateTime.Now.Year - birthDate.Year);

                    //If the birthday is before today, increment the year accordingly
                    if (birthDate < DateTime.Now)
                    {
                        birthDate.AddYears(1);
                    }

                    parsedBirthdays.Add(birthdayChunks[0], birthDate);

                    if (birthDate.Month == DateTime.Now.Month && birthDate.Day == DateTime.Now.Day)
                    {
                        todayBirthdayNames.Add(birthdayChunks[0]);
                    }

                    if (birthDate < nextClosestBirthday.Item2)
                    {
                        nextClosestBirthday.Item1 = birthdayChunks[0];
                        nextClosestBirthday.Item2 = birthDate;
                    }
                }
                catch (ArgumentException)
                {
                    await ReplyAsync("Either a duplicate name was submitted in the birthday file or"
                        + " there was an incorrectly formatted birthday entry.");
                    return;
                }
                catch (FormatException)
                {
                    await ReplyAsync("There was an incorrectly formatted birthday entry.");
                    return;
                }
            }

            if (todayBirthdayNames.Count == 1)
            {
                await ReplyAsync("It's " + todayBirthdayNames[0] + "'s birthday!");
            }
            else if (todayBirthdayNames.Count > 1)
            {
                //Make sure comma doesn't end up at the end of the string of names
                string result = todayBirthdayNames[0];
                todayBirthdayNames.RemoveAt(0);

                await ReplyAsync("There are " + todayBirthdayNames.Count + " birthdays today!\n");

                foreach (string name in todayBirthdayNames)
                {
                    result += ", " + name;
                }
                await ReplyAsync(result);
            }
            else
            {
                if (nextClosestBirthday.Item1 != "")
                {
                    await ReplyAsync(nextClosestBirthday.Item1 + " has the next birthday in "
                        + (nextClosestBirthday.Item2 - DateTime.Now).Days + " days");
                }
            }
        }
        [Discord.Commands.Command("Birthday All"), Summary("Gets all birthdays from file")]
        [Discord.Commands.Alias("birthday all", "bday all", "bdayall")]
        [Discord.Commands.RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task birthdayAll()
        {
            string[] birthdayLines = File.ReadAllLines(filePath);
            string conCat = "";
            var embed = new EmbedBuilder()
                .WithColor(Color.DarkerGrey);
            for(int i = 0; i < birthdayLines.Length; i++)
            {
                if(i % 50 == 0 && i != 0)
                {
                    embed.WithDescription(conCat);
                    await ReplyAsync(embed: embed.Build());
                    embed = new EmbedBuilder()
                        .WithAuthor("All Birthdays on File Cont.")
                        .WithColor(Color.DarkerGrey);
                    conCat = "";
                }

                conCat = string.Concat(conCat, "\n", birthdayLines[i].Replace(' ', ','));
            }
            embed.WithDescription(conCat);
            await ReplyAsync(embed: embed.Build());
            return;
        }
    }
}
