import { mergeProps, useSSRContext } from "vue";
import { ssrRenderAttrs, ssrInterpolate, ssrRenderComponent } from "vue/server-renderer";
import { _ as _export_sfc, V as VIcon } from "../server.mjs";
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
const _sfc_main = {};
function _sfc_ssrRender(_ctx, _push, _parent, _attrs) {
  _push(`<div${ssrRenderAttrs(mergeProps({ class: "h-100 d-flex align-center justify-md-space-between justify-center" }, _attrs))}><span class="d-flex align-center text-medium-emphasis"> © ${ssrInterpolate((/* @__PURE__ */ new Date()).getFullYear())} Made With `);
  _push(ssrRenderComponent(VIcon, {
    icon: "tabler-heart-filled",
    color: "error",
    size: "1.25rem",
    class: "mx-1"
  }, null, _parent));
  _push(` By <a href="https://pixinvent.com" target="_blank" rel="noopener noreferrer" class="text-primary ms-1">Pixinvent</a></span><span class="d-md-flex gap-x-4 text-primary d-none"><a href="https://themeforest.net/licenses/standard" target="noopener noreferrer">License</a><a href="https://1.envato.market/pixinvent_portfolio" target="noopener noreferrer">More Themes</a><a href="https://demos.pixinvent.com/vuexy-vuejs-admin-template/documentation/" target="noopener noreferrer">Documentation</a><a href="https://pixinvent.ticksy.com/" target="noopener noreferrer">Support</a></span></div>`);
}
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/components/Footer.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
const Footer = /* @__PURE__ */ _export_sfc(_sfc_main, [["ssrRender", _sfc_ssrRender]]);
export {
  Footer as default
};
