import { ComputedRef, MaybeRef } from 'vue'
export type LayoutKey = "blank" | "components-default-layout-with-horizontal-nav" | "components-default-layout-with-vertical-nav" | "components-footer" | "components-nav-bar-notifications" | "components-nav-search-bar" | "components-navbar-shortcuts" | "components-navbar-theme-switcher" | "components-user-profile" | "default"
declare module "../../node_modules/.pnpm/nuxt@3.10.3_@types+node@20.11.24_eslint@8.57.0_sass@1.71.1_stylelint@16.2.1_typescript@5.3.3_vite@5.1.4_xml2js@0.6.2/node_modules/nuxt/dist/pages/runtime/composables" {
  interface PageMeta {
    layout?: MaybeRef<LayoutKey | false> | ComputedRef<LayoutKey | false>
  }
}