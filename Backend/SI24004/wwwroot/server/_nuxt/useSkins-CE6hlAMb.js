import { computed, h } from "vue";
import { G as useConfigStore, H as VThemeProvider, I as AppContentLayoutNav } from "../server.mjs";
const useSkins = () => {
  const configStore = useConfigStore();
  const layoutAttrs = computed(() => ({
    verticalNavAttrs: {
      wrapper: h(VThemeProvider, { tag: "aside" }),
      wrapperProps: {
        withBackground: true,
        theme: configStore.isVerticalNavSemiDark && configStore.appContentLayoutNav === AppContentLayoutNav.Vertical ? "dark" : void 0
      }
    }
  }));
  const injectSkinClasses = () => {
  };
  return {
    injectSkinClasses,
    layoutAttrs
  };
};
export {
  useSkins as u
};
