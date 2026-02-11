using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACMESharp.HTTP
{
    /// <summary>
    /// A collection of HTTP header <see cref="Link">Link</see> objects.
    /// </summary>
    /// <remarks>
    /// This collection implements multiple generic-typed enumerable interfaces
    /// but for backward compatibility, the default implemenation, i.e. the
    /// IEnumerable non-generic implementation, returns a sequence of strings
    /// which are the complete, formatted Link values.
    /// </remarks>
    public class LinkCollection : IEnumerable<string>, IEnumerable<Link>, ILookup<string, string>
    {
        private List<Link> _Links = new List<Link>();

        public LinkCollection()
        { }

        /// <summary>
        /// Initializes a new <see cref="LinkCollection"/> from a sequence of <see cref="Link"/> instances.
        /// </summary>
        /// <param name="links">It's OK to provide a null value.</param>
        public LinkCollection(IEnumerable<Link> links)
        {
            if (links != null)
            {
                foreach (var l in links)
                {
                    Add(l);
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="LinkCollection"/> from a sequence of link header string values.
        /// </summary>
        /// <param name="linkValues">It's OK to provide a null value.</param>
        public LinkCollection(IEnumerable<string>? linkValues)
        {
            if (linkValues != null)
            {
                foreach (var lv in linkValues)
                {
                    Add(new Link(lv));
                }
            }
        }

        public IEnumerable<string> this[string key]
        {
            get
            {
                return _Links.Where(x => x.Relation == key).Select(x => x.Uri);
            }
        }

        public int Count
        {
            get
            {
                return _Links.Count;
            }
        }

        public void Add(Link link)
        {
            _Links.Add(link);
        }

        public Link? GetFirstOrDefault(string key)
        {
            return _Links.FirstOrDefault(x => x.Relation == key);
        }

        public bool Contains(string key)
        {
            return GetFirstOrDefault(key) != null;
        }

        IEnumerator<Link> IEnumerable<Link>.GetEnumerator()
        {
            return _Links.GetEnumerator();
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            foreach (var l in _Links)
                yield return l.Value;
        }

        IEnumerator<IGrouping<string, string>> IEnumerable<IGrouping<string, string>>.GetEnumerator()
        {
            return _Links.ToLookup(x => x.Relation, x => x.Uri).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }


    }
}
