using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace CapitalWordCounter
{
    public struct CacheItem
    {
        public string WordCount { get; set; }
        public string FilePath { get; set; }

        public CacheItem(string value, string filePath)
        {
            WordCount = value;
            FilePath = filePath;
        }
    }

    public class CWC_Cache
    {
    //=============================================================================
    // *** CWC CACHE ATTRIBUTES *** //
    //=============================================================================
        private readonly int _capacity;
        private readonly Dictionary<string, LinkedListNode<CacheItem>> _cacheMap;
        private readonly LinkedList<CacheItem> _lruList;
        private readonly object _cacheLock = new object();

        //==================================================================
        // *** CWC CACHE CONSTRUCTOR *** //
        //==================================================================
        /// <summary>
        /// CapitalWordCounter Cache Constructor.
        /// </summary>
        //====================-----------------------=======================
        public CWC_Cache(int capacity)
        {
            try
            {
                if (capacity <= 0)
                    throw new ArgumentException("  !!! The capacity of cache must be set to a value greater than zero.");

                _capacity = capacity;
                _cacheMap = new Dictionary<string, LinkedListNode<CacheItem>>();
                _lruList = new LinkedList<CacheItem>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("=> The capacity of cache was set to a default value of 1024.");
                _capacity = 1024;
            }
        }

    //====================================================================================================================================
    // *** CWC CACHE MANAGEMENT FUNCTIONS *** //
    //====================================================================================================================================
    //
    //
        //===============================================================================================================================
        /// <summary>
        /// Attempts to retrieve the word count value for the specified file from the cache using the <see cref="Get"/> method.<br />
        /// If the file is not found in the cache, it creates a new cache item for the file using the <see cref="Create"/> method.
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve or create the cache item for.</param>
        /// <param name="root">The root directory of the server files.</param>
        /// <param name="createItem">The function used to generate the word count value for the file if it needs to be created.</param>
        /// <returns>The word count value for the specified file.</returns>
        //===============================================================================================================================
        public string GetOrCreate(string fileName, string root, Func<string, string> createItem)
        {
            lock (_cacheLock)
            {
                try
                {
                    if (_cacheMap.ContainsKey(fileName))
                    {
                        return Get(fileName);
                    }
                    else
                    {
                        return Create(fileName, root, createItem);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    throw;
                }
            }
        }


        //=======================================================================================================================
        /// <summary>
        /// Attempts to retrieve the cached word count value for the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve the word count for.</param>
        /// <returns>The word count value if the file is found in the cache, otherwise throws a KeyNotFoundException.</returns>
        //=======================================================================================================================
        private string Get(string fileName)
        {
            try
            {
                if (_cacheMap.TryGetValue(fileName, out LinkedListNode<CacheItem> curnode))
                {
                    SetToMostRecent(curnode);
                    Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Response retrieved from cache for: [{fileName}]");
                    return curnode.Value.WordCount;
                }
                else
                {
                    throw new KeyNotFoundException($"  x Value for key [{fileName}] not found!");
                }
            }
            catch(Exception e)
            {
                Console.Write(e.ToString());
                throw;
            }
        }


        //========================================================================================================================
        /// <summary>
        /// Creates a new cache item for the specified file if it doesn't exist in the cache, and retrieves its word count value.
        /// </summary>
        /// <param name="fileName">The name of the file to create the cache item for.</param>
        /// <param name="root">The root directory of the server files.</param>
        /// <param name="createItem">The function used to generate the word count value for the file.</param>
        /// <returns>The word count value for the specified file.</returns>
        //========================================================================================================================
        private string Create(string fileName, string root, Func<string, string> createItem)
        {
            try
            {
                if (_cacheMap.Count >= _capacity)
                {
                    EvictItem();
                }

                string filePath = null;

                Stopwatch swSearchFile = new Stopwatch();
                swSearchFile.Start();
                filePath = CWC_Server.SearchFile(root, fileName);
                swSearchFile.Stop();
                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : File [{fileName}] found in {swSearchFile.ElapsedMilliseconds}ms");

                Stopwatch swWordCount = Stopwatch.StartNew();
                string wordCount = createItem(filePath);
                swWordCount.Stop();
                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Counting done in {swWordCount.ElapsedMilliseconds}ms for [{fileName}] --> Word Count: {wordCount}");


                LinkedListNode<CacheItem> newNode = new LinkedListNode<CacheItem>(new CacheItem(wordCount, filePath));
                _lruList.AddFirst(newNode);
                _cacheMap[fileName] = newNode;

                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Added new item to cache > [File Path: {filePath}]");
                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : New number of items in cache map: {_cacheMap.Count}/{_capacity}; New number of items in LRU list: {_lruList.Count}");

                return wordCount;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }


        //===========================================================================
        /// <summary>
        /// Moves the specified node to the most recent position in the LRU list.
        /// </summary>
        /// <param name="node">The node to be moved.</param>
        //===========================================================================
        private void SetToMostRecent(LinkedListNode<CacheItem> node)
        {
            _lruList.Remove(node);
            _lruList.AddFirst(node);
        }

        //===========================================================================
        /// <summary>
        /// Evicts the least recently used item from the cache.
        /// </summary>
        //===========================================================================
        private void EvictItem()
        {
            LinkedListNode<CacheItem>? nodeToRemove = _lruList.Last;

            if (nodeToRemove != null)
            {
                _cacheMap.Remove(Path.GetFileName(nodeToRemove.Value.FilePath));
                _lruList.RemoveLast();
                Console.WriteLine($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] : Evicted an item from cache > [[File Path: {nodeToRemove.Value.FilePath}]");
            }
        }

        //===========================================================================
        /// <summary>
        /// Checks if the specified key exists in the cache.
        /// </summary>
        /// <param name="key">The key to check for existence.</param>
        /// <returns>True if the key exists in the cache, otherwise false.</returns>
        //===========================================================================
        public bool ExistsInCache(string key)
        {
            return _cacheMap.ContainsKey(key);
        }

    //====================================================================================================================================
    // *** END *** //
    //====================================================================================================================================

    }
}
