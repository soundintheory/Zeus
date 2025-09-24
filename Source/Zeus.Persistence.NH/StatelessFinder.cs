using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;
using System.Reflection;
using Zeus.ContentProperties;
using Zeus.Linq;
using Zeus.Persistence.NH.Linq;

namespace Zeus.Persistence.NH
{
	public class StatelessFinder : IStatelessFinder
	{
        #region Fields

        private readonly ISessionProvider _sessionProvider;

        private readonly IStatelessSession _session;

		#endregion

		#region Constructor

		public StatelessFinder(ISessionProvider sessionProvider)
		{
			_sessionProvider = sessionProvider;
			_session = sessionProvider.OpenStatelessSession();
		}

		#endregion

		#region IFinder<T> Members

		public IQueryable<T> Query<T>()
		{
            return _session.Query<T>();
		}

		public IQueryable<T> QueryItems<T>()
			where T : ContentItem
		{
			return Query<T>().FetchMany(x => x.Details);
		}

		public IQueryable<ContentItem> QueryItems()
		{
			return Query<ContentItem>();
		}

		public IQueryable<PropertyData> QueryDetails()
		{
			return QueryDetails<PropertyData>();
		}

		public IQueryable<T> QueryDetails<T>()
			where T : PropertyData
		{
			return Query<T>();
		}

		public IQueryable<PropertyCollection> QueryDetailCollections()
		{
			return Query<PropertyCollection>();
		}

		public IQueryable Query(Type resultType)
		{
			MethodInfo genericQueryMethod = GetType().GetMethod("Query", Type.EmptyTypes).MakeGenericMethod(resultType);
			return (IQueryable)genericQueryMethod.Invoke(this, null);
		}

		public TKey Insert<TKey>(object entity)
		{
			return (TKey)_session.Insert(entity);
		}

        public void Update(object entity)
        {
            _session.Update(entity);
        }

        public void Delete(object entity)
        {
            _session.Delete(entity);
        }

        public ITransaction BeginTransaction()
        {
            return new Transaction(_session);
        }

        public IStatelessFinder CreateStateless()
        {
            return new StatelessFinder(_sessionProvider);
        }

        public void Dispose()
        {
            if (_session != null)
			{
				_session.Dispose();
			}
        }

        #endregion
    }

}
