import { defineComponent, defineAsyncComponent, createVNode, resolveDynamicComponent, unref, mergeProps, withCtx, renderSlot, useSSRContext } from "vue";
import { u as useSkins } from "./useSkins-CE6hlAMb.js";
import { ssrRenderVNode, ssrRenderSlot } from "vue/server-renderer";
import { G as useConfigStore, ab as switchToVerticalNavOnLtOverlayNavBreakpoint, I as AppContentLayoutNav } from "../server.mjs";
import "#internal/nitro";
import "ofetch";
import "hookable";
import "unctx";
import "h3";
import "ufo";
import "defu";
import "devalue";
import "cookie-es";
import "@antfu/utils";
import "axios";
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "default",
  __ssrInlineRender: true,
  setup(__props) {
    const DefaultLayoutWithHorizontalNav = defineAsyncComponent(() => import("./DefaultLayoutWithHorizontalNav-DFdP7iYn.js"));
    const DefaultLayoutWithVerticalNav = defineAsyncComponent(() => import("./DefaultLayoutWithVerticalNav-NioSZP5z.js"));
    const configStore = useConfigStore();
    switchToVerticalNavOnLtOverlayNavBreakpoint();
    const { layoutAttrs, injectSkinClasses } = useSkins();
    injectSkinClasses();
    return (_ctx, _push, _parent, _attrs) => {
      ssrRenderVNode(_push, createVNode(resolveDynamicComponent(unref(configStore).appContentLayoutNav === unref(AppContentLayoutNav).Vertical ? unref(DefaultLayoutWithVerticalNav) : unref(DefaultLayoutWithHorizontalNav)), mergeProps(unref(layoutAttrs), _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            ssrRenderSlot(_ctx.$slots, "default", {}, null, _push2, _parent2, _scopeId);
          } else {
            return [
              renderSlot(_ctx.$slots, "default")
            ];
          }
        }),
        _: 3
      }), _parent);
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/default.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
