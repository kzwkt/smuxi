/*
 * Smuxi - Smart MUltipleXed Irc
 *
 * Copyright (c) 2005-2006, 2008, 2012-2013, 2015 Mirco Bauer <meebey@meebey.net>
 *
 * Full GPL License: <http://www.gnu.org/licenses/gpl.txt>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
 */

using System;
using System.Reflection;
using NDesk.Options;
using Smuxi.Common;

namespace Smuxi.Frontend.Gnome
{
    public class CommandLineInterface : MarshalByRefObject
    {
        public void OpenLink(string link)
        {
            Frontend.OpenLink(new Uri(link));
        }

        public override object InitializeLifetimeService()
        {
            // live forever
            return null;
        }
    }

    public class MainClass
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        static readonly string LibraryTextDomain = "smuxi-frontend-gnome";
        static SingleApplicationInstance<CommandLineInterface> Instance { get; set; }

        public static void Main(string[] args)
        {
            var debug = false;
            var link = String.Empty;
            var engine = String.Empty;
            var options = new OptionSet();
            options.Add(
                "d|debug",
                _("Enable debug output"),
                v => {
                    debug = true;
                }
            );
            options.Add(
                "h|help",
                _("Show this help"),
                v => {
                    Console.WriteLine("Usage: smuxi-frontend-gnome [options]");
                    Console.WriteLine();
                    Console.WriteLine(_("Options:"));
                    options.WriteOptionDescriptions(Console.Out);
                    Environment.Exit(0);
                }
            );
            options.Add(
                "e|engine=",
                _("Connect to engine"),
                v => {
                    engine = v;
                }
            );
            options.Add(
                "open|open-link=",
                _("Opens the specified link in Smuxi"),
                v => {
                    link = v;
                }
            );

            try {
                options.Parse(args);

#if LOG4NET
                // initialize log level
                log4net.Repository.ILoggerRepository repo = log4net.LogManager.GetRepository();
                if (debug) {
                    repo.Threshold = log4net.Core.Level.Debug;
                } else {
                    repo.Threshold = log4net.Core.Level.Info;
                }
#endif

                Instance = new SingleApplicationInstance<CommandLineInterface>();
                if (Instance.IsFirstInstance) {
                    Instance.FirstInstance = new CommandLineInterface();
                } else {
                    if (!String.IsNullOrEmpty(link)) {
                        Instance.FirstInstance.OpenLink(link);
                    }
                    return;
                }

                Frontend.Init(args, engine);
            } catch (Exception e) {
#if LOG4NET
                _Logger.Fatal(e);
#endif
                // when Gtk# receives an exception it is not usable/relyable anymore! 
                // except the exception was thrown in Frontend.Init() itself
                if (Frontend.IsGtkInitialized && !Frontend.InGtkApplicationRun) {
                    Frontend.ShowException(e);
                }
                
                // rethrow the exception for console output
                throw;
            }
        }

        static string _(string msg)
        {
            return LibraryCatalog.GetString(msg, LibraryTextDomain);
        }
    }
}
