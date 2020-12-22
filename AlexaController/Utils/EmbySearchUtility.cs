using System;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;

// ReSharper disable once ExcessiveIndentation

namespace AlexaController.Utils
{
    public class EmbySearchUtility 
    {
        private ILibraryManager LibraryManager { get; }
        private IUserManager UserManager       { get; }

        protected EmbySearchUtility(ILibraryManager libMan, IUserManager userManager) 
        {
            LibraryManager = libMan;
            UserManager = userManager;
        }
                
        public BaseItem QuerySpeechResultItem(string searchName, string[] type)
        {
            EmbyServerEntryPoint.Instance.Log.Info("Beginning item search");

            var result = LibraryManager.GetItemIds(new InternalItemsQuery
            {
                Name             = searchName,
                Recursive = true,
                IncludeItemTypes = type,
                User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
               
            });

            //Remove "The" from search Term
            if (!result.Any())
            {
                try
                {
                    if (searchName.ToLower().StartsWith("the "))
                    {
                        var query = searchName.Substring(4);
                        
                        var queryResult = LibraryManager.QueryItems(new InternalItemsQuery
                        {
                            Name             = query,
                            IncludeItemTypes = type,
                            User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                        });

                        if (queryResult.Items.Any())
                        {
                            EmbyServerEntryPoint.Instance.Log.Info("search found: " + queryResult.Items.FirstOrDefault(r => NormalizeQueryString(r.Name).Contains(NormalizeQueryString(query))).Name);
                            return queryResult.Items.FirstOrDefault(r => NormalizeQueryString(r.Name).Contains(NormalizeQueryString(query)));
                        }
                    }
                } catch { }
            }

            //Remove "The" from BaseItem Name
            if (!result.Any())
            {
                //Remove "The" from query and check against request
                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
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
                        EmbyServerEntryPoint.Instance.Log.Info("search found: " + resultItem.Name);
                        return resultItem;
                    }
                }
            }
            
            //Return items that start with the first two letters of search term, removing proceeding  "the"
            if (!result.Any())
            {
                var query = searchName.ToLower().StartsWith("the ") ? searchName.Substring(4, 6) : searchName.Substring(0);
                
                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    NameStartsWith   = query,
                    User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    BaseItem resultItem = null;

                    Parallel.ForEach(queryResult.Items, (item, state) =>
                    {
                        // The user may have used the phrase "part 2", the movie name is "part ii"
                        if (item.Name.ToLower().Replace(" ii", " 2").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                        if (item.Name.ToLower().Replace(" iii", " 3").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                        if (item.Name.ToLower().Replace(" iv", " 4").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                        if (item.Name.ToLower().Replace(" v", " 5").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }
                        if (item.Name.ToLower().Replace("part ii", "2").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }
                        if (item.Name.ToLower().Replace("part iii", "3").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                        if (item.Name.ToLower().Replace("part iv", "4").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                        if (item.Name.ToLower().Replace("part v", "5").Equals(searchName))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }



                        if (item.Name.ToLower().Replace("vol.", string.Empty)
                            .Equals(searchName.ToLowerInvariant().Replace("volume", string.Empty),
                                StringComparison.InvariantCultureIgnoreCase))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                        if (NormalizeQueryString(item.Name).Contains(NormalizeQueryString(searchName)))
                        {
                            state.Break();
                            resultItem = item;
                            //return item;
                        }

                    });

                    if (!(resultItem is null))
                    {
                        EmbyServerEntryPoint.Instance.Log.Info("search found: " + resultItem.Name);
                        return resultItem;
                    }
                }
            }
            
            if (!result.Any()) //split name "" - Starts with string first and second word
            {
                var query = searchName.ToLower().StartsWith("the ") ? searchName.Substring(4) : searchName;

                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    SearchTerm       = query,
                    User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    EmbyServerEntryPoint.Instance.Log.Info("search found: " + queryResult.Items
                                                               .FirstOrDefault(r =>
                                                                   NormalizeQueryString(r.Name)
                                                                       .Contains(NormalizeQueryString(searchName)))
                                                               .Name);
                    return queryResult.Items.FirstOrDefault(r => NormalizeQueryString(r.Name).Contains(NormalizeQueryString(searchName)));
                }
            }

            if (!result.Any())
            {
                var query = searchName.ToLower().StartsWith("the ") ? searchName.Substring(4, 6) : searchName.Substring(0, 2);
                
                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    NameStartsWith   = query,
                    User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    EmbyServerEntryPoint.Instance.Log.Info("search found: " + queryResult.Items
                                                               .FirstOrDefault(item =>
                                                                   item.Name.ToLower().Contains(searchName.ToLower()))
                                                               .Name);
                    return queryResult.Items.FirstOrDefault(item => item.Name.ToLower().Contains(searchName.ToLower()));
                }
            }

            //User has definitely mis-spoke the name - a Hail Mary search
            if (!result.Any())
            {
                var query = searchName.Replace("the", string.Empty);

                var queryResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = type,
                    Recursive = true,
                    NameStartsWith   = query,
                    User             = UserManager.Users.FirstOrDefault(user => user.Policy.IsAdministrator)
                });

                if (queryResult.Items.Any())
                {
                    EmbyServerEntryPoint.Instance.Log.Info("search found: " + queryResult.Items
                                                               .FirstOrDefault(item =>
                                                                   item.Name.ToLower().Contains(searchName.ToLower()))
                                                               .Name);
                    return queryResult.Items.FirstOrDefault(item => item.Name.ToLower().Contains(searchName.ToLower()));
                }
            }

            if (!result.Any())
            {
                return null;
            }

            EmbyServerEntryPoint.Instance.Log.Info("search found: " +
                                                   LibraryManager.GetItemById(result.FirstOrDefault()).Name);

            return LibraryManager.GetItemById(result.FirstOrDefault());
        }

        private static string NormalizeQueryString(string sample)
        {
            var result = sample
                .Replace("-", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("&", string.Empty)
                .Replace("and", string.Empty)
                .Replace(":", string.Empty)
                .Replace("1", "one")
                .Replace("...", string.Empty);
            return result.Replace(" ", string.Empty).ToLower();
        }
    }
}
