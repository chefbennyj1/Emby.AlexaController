using System;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

// ReSharper disable once ExcessiveIndentation

namespace AlexaController
{
    public class ServerBaseItemQueryResolution
    {
        private ILibraryManager LibraryManager { get; }
        private IUserManager UserManager { get; }

        protected ServerBaseItemQueryResolution(ILibraryManager libMan, IUserManager userManager)
        {
            LibraryManager = libMan;
            UserManager = userManager;
        }

        public BaseItem QuerySpeechResultItem(string searchName, string[] type)
        {
            ServerController.Instance.Log.Info("Beginning item search");
            ServerController.Instance.Log.Info($"Search Name: {searchName}");
            ServerController.Instance.Log.Info($"Search Type: {type[0]}");

            ServerController.Instance.Log.Info("Query 1:");
            var result = LibraryManager.GetItemIds(new InternalItemsQuery
            {
                Name = searchName,
                Recursive = true,
                IncludeItemTypes = type,
                User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)

            });

            // 
            //Remove "The" from search Term
            if (!result.Any())
            {
                ServerController.Instance.Log.Info("Query 2");
                try
                {
                    if (searchName.ToLower().StartsWith("the "))
                    {
                        var query = searchName.Substring(4);

                        var queryResult = LibraryManager.QueryItems(new InternalItemsQuery
                        {
                            Name = query,
                            IncludeItemTypes = type,
                            User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                        });

                        if (queryResult.Items.Any())
                        {

                            var rv = queryResult.Items.FirstOrDefault(r => NormalizeQueryString(r.Name).Contains(NormalizeQueryString(query)));
                            ServerController.Instance.Log.Info(rv.Name);
                            return rv;
                        }
                    }
                }
                catch { }
            }

            //Remove "The" from BaseItem Name
            if (!result.Any())
            {
                //Remove "The" from query and check against request
                ServerController.Instance.Log.Info("Query 3");
                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    BaseItem resultItem = null;
                    Parallel.ForEach(queryResult.Items, (item, state) => //).foreach (var item in queryResult.Items)
                    {
                        //If the item starts with "The"
                        if (item.Name.ToLower().StartsWith("the "))
                        {
                            var query = item.Name.Substring(4);

                            //Remove "The" from  name
                            if (string.Equals(query, (searchName), StringComparison.CurrentCultureIgnoreCase))
                            {
                                //Logger.Info($"Search 3: found Comparison {searchName} and {query}");
                                resultItem = item;
                                state.Break();
                                //return item;
                            }

                            //Maybe "The" should be there
                            if (string.Equals(NormalizeQueryString(item.Name).ToLowerInvariant(),
                                NormalizeQueryString(searchName).ToLower(), StringComparison.CurrentCultureIgnoreCase))
                            {
                                resultItem = item;
                                state.Break();
                                //return item;
                            }
                        }

                        //Maybe search name can perfectly compare
                        if (string.Equals(NormalizeQueryString(item.Name.ToLowerInvariant()),
                            NormalizeQueryString(searchName.ToLowerInvariant()),
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            resultItem = item;
                            state.Break();
                            //return item;
                        }
                    });
                    if (!(resultItem is null))
                    {
                        var r = resultItem;
                        ServerController.Instance.Log.Info(r.Name);
                        return r;
                    }
                }
            }

            
            if (!result.Any())
            {
                ServerController.Instance.Log.Info("Query 4");
                

                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    
                    foreach (var item in queryResult.Items)
                    {
                        //Check for Roman Numerals in the name, and replace them with numeric string values for name comparison
                        if (item.Name.ToLower().EndsWith(" ii"))
                        {
                            if (item.Name.ToLower().Replace(" ii", " 2").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().EndsWith(" iii"))
                        {
                            if (item.Name.ToLower().Replace(" iii", " 3").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().EndsWith(" iv"))
                        {
                            if (item.Name.ToLower().Replace(" iv", " 4").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().EndsWith(" v"))
                        {
                            if (item.Name.ToLower().Replace(" v", " 5").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().Contains(" part ii"))
                        {
                            if (item.Name.ToLower().Replace("part ii", "2").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().Contains(" part iii"))
                        {
                            if (item.Name.ToLower().Replace("part iii", "3").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().Contains(" part iv"))
                        {
                            if (item.Name.ToLower().Replace("part iv", "4").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().Contains(" part v"))
                        {
                            if (item.Name.ToLower().Replace("part v", "5").Equals(searchName))
                            {
                                return item;
                            }
                        }

                        //Check for Numeric values in the name, and replace them with Roman Numerals for name comparison
                        if (item.Name.ToLower().EndsWith(" ii"))
                        {
                            if (NormalizeQueryString(item.Name.ToLower().Replace(" ii", " 2")).Equals(NormalizeQueryString(searchName)))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().EndsWith(" iii"))
                        {
                            if (NormalizeQueryString(item.Name.ToLower().Replace(" iii", " 3")).Equals(NormalizeQueryString(searchName)))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().EndsWith(" iv"))
                        {
                            if (NormalizeQueryString(item.Name.ToLower().Replace(" iv", " 4")).Equals(NormalizeQueryString(searchName)))
                            {
                                return item;
                            }
                        }

                        if (item.Name.ToLower().EndsWith(" v"))
                        {
                            if (NormalizeQueryString(item.Name.ToLower().Replace(" v", " 5")).Equals(NormalizeQueryString(searchName)))
                            {
                                return item;
                            }
                        }




                        if (NormalizeQueryString(item.Name).Contains(NormalizeQueryString(searchName)))
                        {
                            return item;
                        }

                        if (NormalizeQueryString(item.Name).Equals(NormalizeQueryString(searchName)))
                        {
                            return item;
                        }

                    }

                    
                }
            }

            if (!result.Any()) //split name "" - Starts with string first and second letter
            {
                ServerController.Instance.Log.Info("Query 5");
                var query = searchName.ToLower().StartsWith("the ") ? searchName.Substring(4) : searchName;

                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    //SearchTerm = query,
                    User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    var rv = queryResult.Items.FirstOrDefault(r => NormalizeQueryString(r.Name).Contains(NormalizeQueryString(query)));
                    ServerController.Instance.Log.Info(rv.Name);
                    return rv;
                }
            }

            if (!result.Any())
            {
                var query = searchName.ToLower().StartsWith("the ") ? searchName.Substring(4, 6) : searchName.Substring(0, 2);
                ServerController.Instance.Log.Info("Query 6");
                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    NameStartsWithOrGreater = query,
                    User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    var r = queryResult.Items.FirstOrDefault(item => item.Name.ToLower().Contains(query.ToLower()));
                    ServerController.Instance.Log.Info(r.Name);
                    return r;
                }
            }

            //User has definitely mis-spoke the name - a Hail Mary search items with the same types
            if (!result.Any())
            {
                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    NameStartsWithOrGreater = searchName,
                    User = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    var r =  queryResult.Items.FirstOrDefault(item => item.Name.ToLower().Contains(searchName.ToLower()));
                    ServerController.Instance.Log.Info(r.Name);
                    return r;
                }
            }

            if (!result.Any())
            {
                return null;
            }

            return LibraryManager.GetItemById(result.FirstOrDefault());
        }

        private static string NormalizeQueryString(string sample)
        {
            var result = sample.ToLowerInvariant()

            .Replace("versus", "vs.")
                .Replace("the", string.Empty)
                .Replace("-", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("&", string.Empty)
                .Replace("and", string.Empty)
                .Replace(":", string.Empty)
                .Replace("1", "one")
                .Replace("vol.", string.Empty)
                .Replace("volume", string.Empty)
                .Replace("...", string.Empty)
                .Replace("home theater", string.Empty);
            return result.Replace(" ", string.Empty);
        }
    }
}
