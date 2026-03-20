import { defu } from 'defu'
import type { UseFetchOptions } from 'nuxt/app'

export const useApi: typeof useFetch = <T>(
  url: MaybeRefOrGetter<string>, options: UseFetchOptions<T> = {}) => {

  const defaults: UseFetchOptions<T> = {
    baseURL: useRuntimeConfig().public.apiBase,
    headers: {
      "Content-Type": "application/json",
      "Access-Control-Allow-Origin": "*",
    },
    key: toValue(url),
  }

  // for nice deep defaults, please use unjs/defu
  const params = defu(options, defaults)

  return useFetch(url, params)
}
