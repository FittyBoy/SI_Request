import { _ as __nuxt_component_0, V as VNodeRenderer } from "./VNodeRenderer-Bw61TUGP.js";
import { defineComponent, mergeProps, createVNode, resolveDynamicComponent, useSSRContext, ref, watch, unref, withCtx, openBlock, createBlock, Fragment, renderList, createTextVNode, toDisplayString, withDirectives, renderSlot, vShow, createCommentVNode } from "vue";
import { ssrRenderAttrs, ssrRenderList, ssrRenderVNode, ssrRenderComponent, ssrInterpolate, ssrRenderClass, ssrRenderSlot, ssrRenderStyle } from "vue/server-renderer";
import { J as useRoute, D as useRouter, K as useLayoutConfigStore, L as isNavGroupActive, M as layoutConfig, N as getDynamicI18nProps, O as getComputedNavLinkToProp, P as isNavLinkActive, Q as until, R as useEventListener, t as themeConfig, S as VSpacer } from "../server.mjs";
import Footer from "./Footer-DTgAlwVo.js";
import _sfc_main$7 from "./NavbarThemeSwitcher-D7hOas_S.js";
import _sfc_main$8 from "./UserProfile-gvCVG-UJ.js";
import { c as canViewNavMenuGroup, a as can, _ as _sfc_main$6 } from "./I18n-Dk6xY8D3.js";
import { computePosition, offset, flip, shift } from "@floating-ui/dom";
import "ufo";
import "#internal/nitro";
import "ofetch";
import "hookable";
import "unctx";
import "h3";
import "defu";
import "devalue";
import "cookie-es";
import "@antfu/utils";
import "axios";
import "@casl/vue";
const _sfc_main$5 = /* @__PURE__ */ defineComponent({
  __name: "HorizontalNav",
  __ssrInlineRender: true,
  props: {
    navItems: {}
  },
  setup(__props) {
    const resolveNavItemComponent = (item) => {
      if ("children" in item)
        return _sfc_main$4;
      return _sfc_main$2;
    };
    return (_ctx, _push, _parent, _attrs) => {
      _push(`<ul${ssrRenderAttrs(mergeProps({ class: "nav-items" }, _attrs))}><!--[-->`);
      ssrRenderList(_ctx.navItems, (item, index) => {
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(resolveNavItemComponent(item)), {
          key: index,
          item
        }, null), _parent);
      });
      _push(`<!--]--></ul>`);
    };
  }
});
const _sfc_setup$5 = _sfc_main$5.setup;
_sfc_main$5.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/HorizontalNav.vue");
  return _sfc_setup$5 ? _sfc_setup$5(props, ctx) : void 0;
};
const _sfc_main$4 = /* @__PURE__ */ defineComponent({
  ...{
    name: "HorizontalNavGroup"
  },
  __name: "HorizontalNavGroup",
  __ssrInlineRender: true,
  props: {
    item: {},
    childrenAtEnd: { type: Boolean, default: false },
    isSubItem: { type: Boolean, default: false }
  },
  setup(__props) {
    const props = __props;
    const route = useRoute();
    const router = useRouter();
    const configStore = useLayoutConfigStore();
    const isGroupActive = ref(false);
    watch(() => route.path, () => {
      const isActive = isNavGroupActive(props.item.children, router);
      isGroupActive.value = isActive;
    }, { immediate: true });
    return (_ctx, _push, _parent, _attrs) => {
      if (unref(canViewNavMenuGroup)(_ctx.item)) {
        _push(ssrRenderComponent(unref(_sfc_main$1), mergeProps({
          "is-rtl": unref(configStore).isAppRTL,
          class: ["nav-group", [{
            "active": unref(isGroupActive),
            "children-at-end": _ctx.childrenAtEnd,
            "sub-item": _ctx.isSubItem,
            "disabled": _ctx.item.disable
          }]],
          tag: "li",
          "content-container-tag": "ul",
          "popper-inline-end": _ctx.childrenAtEnd
        }, _attrs), {
          content: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              _push2(`<!--[-->`);
              ssrRenderList(_ctx.item.children, (child) => {
                ssrRenderVNode(_push2, createVNode(resolveDynamicComponent("children" in child ? "HorizontalNavGroup" : unref(_sfc_main$2)), {
                  key: child.title,
                  item: child,
                  "children-at-end": "",
                  "is-sub-item": ""
                }, null), _parent2, _scopeId);
              });
              _push2(`<!--]-->`);
            } else {
              return [
                (openBlock(true), createBlock(Fragment, null, renderList(_ctx.item.children, (child) => {
                  return openBlock(), createBlock(resolveDynamicComponent("children" in child ? "HorizontalNavGroup" : unref(_sfc_main$2)), {
                    key: child.title,
                    item: child,
                    "children-at-end": "",
                    "is-sub-item": ""
                  }, null, 8, ["item"]);
                }), 128))
              ];
            }
          }),
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              _push2(`<div class="nav-group-label"${_scopeId}>`);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({ class: "nav-item-icon" }, _ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps), null), _parent2, _scopeId);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps(unref(getDynamicI18nProps)(_ctx.item.title, "span"), { class: "nav-item-title" }), {
                default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(`${ssrInterpolate(_ctx.item.title)}`);
                  } else {
                    return [
                      createTextVNode(toDisplayString(_ctx.item.title), 1)
                    ];
                  }
                }),
                _: 1
              }), _parent2, _scopeId);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps(unref(layoutConfig).icons.chevronDown, { class: "nav-group-arrow" }), null), _parent2, _scopeId);
              _push2(`</div>`);
            } else {
              return [
                createVNode("div", { class: "nav-group-label" }, [
                  (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({ class: "nav-item-icon" }, _ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps), null, 16)),
                  (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps(unref(getDynamicI18nProps)(_ctx.item.title, "span"), { class: "nav-item-title" }), {
                    default: withCtx(() => [
                      createTextVNode(toDisplayString(_ctx.item.title), 1)
                    ]),
                    _: 1
                  }, 16)),
                  (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps(unref(layoutConfig).icons.chevronDown, { class: "nav-group-arrow" }), null, 16))
                ])
              ];
            }
          }),
          _: 1
        }, _parent));
      } else {
        _push(`<!---->`);
      }
    };
  }
});
const _sfc_setup$4 = _sfc_main$4.setup;
_sfc_main$4.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/HorizontalNavGroup.vue");
  return _sfc_setup$4 ? _sfc_setup$4(props, ctx) : void 0;
};
const _sfc_main$3 = /* @__PURE__ */ defineComponent({
  __name: "HorizontalNavLayout",
  __ssrInlineRender: true,
  props: {
    navItems: {}
  },
  setup(__props) {
    const configStore = useLayoutConfigStore();
    return (_ctx, _push, _parent, _attrs) => {
      _push(`<div${ssrRenderAttrs(mergeProps({
        class: ["layout-wrapper", unref(configStore)._layoutClasses]
      }, _attrs))}><div class="${ssrRenderClass([unref(configStore).isNavbarBlurEnabled && "header-blur", "layout-navbar-and-nav-container"])}"><div class="layout-navbar"><div class="navbar-content-container">`);
      ssrRenderSlot(_ctx.$slots, "navbar", {}, null, _push, _parent);
      _push(`</div></div><div class="layout-horizontal-nav"><div class="horizontal-nav-content-container">`);
      _push(ssrRenderComponent(unref(_sfc_main$5), { "nav-items": _ctx.navItems }, null, _parent));
      _push(`</div></div></div><main class="layout-page-content">`);
      ssrRenderSlot(_ctx.$slots, "default", {}, null, _push, _parent);
      _push(`</main><footer class="layout-footer"><div class="footer-content-container">`);
      ssrRenderSlot(_ctx.$slots, "footer", {}, null, _push, _parent);
      _push(`</div></footer></div>`);
    };
  }
});
const _sfc_setup$3 = _sfc_main$3.setup;
_sfc_main$3.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/HorizontalNavLayout.vue");
  return _sfc_setup$3 ? _sfc_setup$3(props, ctx) : void 0;
};
const _sfc_main$2 = /* @__PURE__ */ defineComponent({
  __name: "HorizontalNavLink",
  __ssrInlineRender: true,
  props: {
    item: {},
    isSubItem: { type: Boolean, default: false }
  },
  setup(__props) {
    const props = __props;
    return (_ctx, _push, _parent, _attrs) => {
      if (unref(can)(_ctx.item.action, _ctx.item.subject)) {
        _push(`<li${ssrRenderAttrs(mergeProps({
          class: ["nav-link", [{
            "sub-item": props.isSubItem,
            "disabled": _ctx.item.disable
          }]]
        }, _attrs))}>`);
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(_ctx.item.to ? unref(__nuxt_component_0) : "a"), mergeProps(unref(getComputedNavLinkToProp)(_ctx.item), {
          class: { "router-link-active router-link-exact-active": unref(isNavLinkActive)(_ctx.item, _ctx.$router) }
        }), {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({ class: "nav-item-icon" }, _ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps), null), _parent2, _scopeId);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({ class: "nav-item-title" }, unref(getDynamicI18nProps)(_ctx.item.title, "span")), {
                default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(`${ssrInterpolate(_ctx.item.title)}`);
                  } else {
                    return [
                      createTextVNode(toDisplayString(_ctx.item.title), 1)
                    ];
                  }
                }),
                _: 1
              }), _parent2, _scopeId);
            } else {
              return [
                (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({ class: "nav-item-icon" }, _ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps), null, 16)),
                (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({ class: "nav-item-title" }, unref(getDynamicI18nProps)(_ctx.item.title, "span")), {
                  default: withCtx(() => [
                    createTextVNode(toDisplayString(_ctx.item.title), 1)
                  ]),
                  _: 1
                }, 16))
              ];
            }
          }),
          _: 1
        }), _parent);
        _push(`</li>`);
      } else {
        _push(`<!---->`);
      }
    };
  }
});
const _sfc_setup$2 = _sfc_main$2.setup;
_sfc_main$2.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/HorizontalNavLink.vue");
  return _sfc_setup$2 ? _sfc_setup$2(props, ctx) : void 0;
};
const _sfc_main$1 = /* @__PURE__ */ defineComponent({
  __name: "HorizontalNavPopper",
  __ssrInlineRender: true,
  props: {
    popperInlineEnd: { type: Boolean, default: false },
    tag: { default: "div" },
    contentContainerTag: { default: "div" },
    isRtl: { type: Boolean }
  },
  setup(__props) {
    const props = __props;
    const configStore = useLayoutConfigStore();
    const refPopperContainer = ref();
    const refPopper = ref();
    const popperContentStyles = ref({
      left: "0px",
      top: "0px"
    });
    const updatePopper = async () => {
      if (refPopperContainer.value !== void 0 && refPopper.value !== void 0) {
        const { x, y } = await computePosition(
          refPopperContainer.value,
          refPopper.value,
          {
            placement: props.popperInlineEnd ? props.isRtl ? "left-start" : "right-start" : "bottom-start",
            middleware: [
              ...configStore.horizontalNavPopoverOffset ? [offset(configStore.horizontalNavPopoverOffset)] : [],
              flip({ boundary: (void 0).querySelector("body"), padding: { bottom: 16 } }),
              shift({ boundary: (void 0).querySelector("body"), padding: { bottom: 16 } })
            ]
            /*
                  ℹ️ Why we are not using fixed positioning?
            
                  `position: fixed` doesn't work as expected when some CSS properties like `transform` is applied on its parent element.
                  Docs: https://developer.mozilla.org/en-US/docs/Web/CSS/position#values <= See `fixed` value description
            
                  Hence, when we use transitions where transition apply `transform` on its parent element, fixed positioning will not work.
                  (Popper content moves away from the element when parent element transition)
            
                  To avoid this, we use `position: absolute` instead of `position: fixed`.
            
                  NOTE: This issue starts from third level children (Top Level > Sub item > Sub item).
                */
            // strategy: 'fixed',
          }
        );
        popperContentStyles.value.left = `${x}px`;
        popperContentStyles.value.top = `${y}px`;
      }
    };
    until(() => configStore.horizontalNavType).toMatch((type) => type === "static").then(() => {
      useEventListener("scroll", updatePopper);
    });
    const isContentShown = ref(false);
    const showContent = () => {
      isContentShown.value = true;
      updatePopper();
    };
    const hideContent = () => {
      isContentShown.value = false;
    };
    watch(
      [
        () => configStore.isAppRTL,
        () => configStore.appContentWidth
      ],
      updatePopper
    );
    const route = useRoute();
    watch(() => route.fullPath, hideContent);
    return (_ctx, _push, _parent, _attrs) => {
      _push(`<div${ssrRenderAttrs(mergeProps({
        class: ["nav-popper", [{
          "popper-inline-end": _ctx.popperInlineEnd,
          "show-content": unref(isContentShown)
        }]]
      }, _attrs))}><div class="popper-triggerer">`);
      ssrRenderSlot(_ctx.$slots, "default", {}, null, _push, _parent);
      _push(`</div>`);
      if (!unref(themeConfig).horizontalNav.transition) {
        _push(`<div class="popper-content" style="${ssrRenderStyle(unref(popperContentStyles))}"><div>`);
        ssrRenderSlot(_ctx.$slots, "content", {}, null, _push, _parent);
        _push(`</div></div>`);
      } else if (typeof unref(themeConfig).horizontalNav.transition === "string") {
        _push(`<div style="${ssrRenderStyle([
          unref(isContentShown) ? null : { display: "none" },
          unref(popperContentStyles)
        ])}" class="popper-content"><div>`);
        ssrRenderSlot(_ctx.$slots, "content", {}, null, _push, _parent);
        _push(`</div></div>`);
      } else {
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(unref(themeConfig).horizontalNav.transition), null, {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              _push2(`<div style="${ssrRenderStyle([
                unref(isContentShown) ? null : { display: "none" },
                unref(popperContentStyles)
              ])}" class="popper-content"${_scopeId}><div${_scopeId}>`);
              ssrRenderSlot(_ctx.$slots, "content", {}, null, _push2, _parent2, _scopeId);
              _push2(`</div></div>`);
            } else {
              return [
                withDirectives(createVNode("div", {
                  ref_key: "refPopper",
                  ref: refPopper,
                  class: "popper-content",
                  style: unref(popperContentStyles),
                  onMouseenter: showContent,
                  onMouseleave: hideContent
                }, [
                  createVNode("div", null, [
                    renderSlot(_ctx.$slots, "content")
                  ])
                ], 36), [
                  [vShow, unref(isContentShown)]
                ])
              ];
            }
          }),
          _: 3
        }), _parent);
      }
      _push(`</div>`);
    };
  }
});
const _sfc_setup$1 = _sfc_main$1.setup;
_sfc_main$1.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/HorizontalNavPopper.vue");
  return _sfc_setup$1 ? _sfc_setup$1(props, ctx) : void 0;
};
const navItems = [
  {
    title: "Home",
    to: { name: "index" },
    icon: { icon: "tabler-smart-home" }
  },
  {
    title: "Second page",
    to: { name: "second-page" },
    icon: { icon: "tabler-file" }
  }
];
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "DefaultLayoutWithHorizontalNav",
  __ssrInlineRender: true,
  setup(__props) {
    return (_ctx, _push, _parent, _attrs) => {
      const _component_NuxtLink = __nuxt_component_0;
      _push(ssrRenderComponent(unref(_sfc_main$3), mergeProps({ "nav-items": unref(navItems) }, _attrs), {
        navbar: withCtx((_, _push2, _parent2, _scopeId) => {
          var _a, _b;
          if (_push2) {
            _push2(ssrRenderComponent(_component_NuxtLink, {
              to: "/",
              class: "app-logo d-flex align-center gap-x-3"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(unref(VNodeRenderer), {
                    nodes: unref(themeConfig).app.logo
                  }, null, _parent3, _scopeId2));
                  _push3(`<h1 class="app-title font-weight-bold leading-normal text-xl text-capitalize"${_scopeId2}>${ssrInterpolate(unref(themeConfig).app.title)}</h1>`);
                } else {
                  return [
                    createVNode(unref(VNodeRenderer), {
                      nodes: unref(themeConfig).app.logo
                    }, null, 8, ["nodes"]),
                    createVNode("h1", { class: "app-title font-weight-bold leading-normal text-xl text-capitalize" }, toDisplayString(unref(themeConfig).app.title), 1)
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
            _push2(ssrRenderComponent(VSpacer, null, null, _parent2, _scopeId));
            if (unref(themeConfig).app.i18n.enable && ((_a = unref(themeConfig).app.i18n.langConfig) == null ? void 0 : _a.length)) {
              _push2(ssrRenderComponent(_sfc_main$6, {
                languages: unref(themeConfig).app.i18n.langConfig
              }, null, _parent2, _scopeId));
            } else {
              _push2(`<!---->`);
            }
            _push2(ssrRenderComponent(_sfc_main$7, { class: "me-2" }, null, _parent2, _scopeId));
            _push2(ssrRenderComponent(_sfc_main$8, null, null, _parent2, _scopeId));
          } else {
            return [
              createVNode(_component_NuxtLink, {
                to: "/",
                class: "app-logo d-flex align-center gap-x-3"
              }, {
                default: withCtx(() => [
                  createVNode(unref(VNodeRenderer), {
                    nodes: unref(themeConfig).app.logo
                  }, null, 8, ["nodes"]),
                  createVNode("h1", { class: "app-title font-weight-bold leading-normal text-xl text-capitalize" }, toDisplayString(unref(themeConfig).app.title), 1)
                ]),
                _: 1
              }),
              createVNode(VSpacer),
              unref(themeConfig).app.i18n.enable && ((_b = unref(themeConfig).app.i18n.langConfig) == null ? void 0 : _b.length) ? (openBlock(), createBlock(_sfc_main$6, {
                key: 0,
                languages: unref(themeConfig).app.i18n.langConfig
              }, null, 8, ["languages"])) : createCommentVNode("", true),
              createVNode(_sfc_main$7, { class: "me-2" }),
              createVNode(_sfc_main$8)
            ];
          }
        }),
        footer: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(Footer, null, null, _parent2, _scopeId));
          } else {
            return [
              createVNode(Footer)
            ];
          }
        }),
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
      }, _parent));
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/components/DefaultLayoutWithHorizontalNav.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
