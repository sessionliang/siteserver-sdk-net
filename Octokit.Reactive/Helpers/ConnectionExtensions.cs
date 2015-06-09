﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Octokit.Reactive.Internal
{
    public static class ConnectionExtensions
    {
        public static IObservable<T> GetAndFlattenAllPages<T>(this IConnection connection, Uri url)
        {
            return GetPages(url, null, (pageUrl, pageParams) => connection.Get<List<T>>(pageUrl, null, null).ToObservable());
        }

        public static IObservable<T> GetAndFlattenAllPages<T>(this IConnection connection, Uri url, IDictionary<string, string> parameters)
        {
            return GetPages(url, parameters, (pageUrl, pageParams) => connection.Get<List<T>>(pageUrl, pageParams, null).ToObservable());
        }

        public static IObservable<T> GetAndFlattenAllPages<T>(this IConnection connection, Uri url, IDictionary<string, string> parameters, string accepts)
        {
            return GetPages(url, parameters, (pageUrl, pageParams) => connection.Get<List<T>>(pageUrl, pageParams, accepts).ToObservable());
        }

        static IObservable<T> GetPages<T>(Uri uri, IDictionary<string, string> parameters,
            Func<Uri, IDictionary<string, string>, IObservable<IApiResponse<List<T>>>> getPageFunc)
        {
            return getPageFunc(uri, parameters).Expand(resp =>
            {
                var nextPageUrl = resp.HttpResponse.ApiInfo.GetNextPageUrl();
                return nextPageUrl == null
                    ? Observable.Empty<IApiResponse<List<T>>>()
                    : Observable.Defer(() => getPageFunc(nextPageUrl, null));
            })
            .Where(resp => resp != null)
            .SelectMany(resp => resp.Body);
        }
    }
}
