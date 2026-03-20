import { defineComponent, h, Transition, useSSRContext, ref, provide, watch, createVNode, resolveDynamicComponent, mergeProps, unref, withCtx, withDirectives, toDisplayString, vShow, openBlock, createBlock, Fragment, renderList, renderSlot, inject, TransitionGroup, createTextVNode, createCommentVNode, toRef, resolveComponent } from "vue";
import { ssrRenderVNode, ssrRenderSlot, ssrRenderComponent, ssrRenderStyle, ssrInterpolate, ssrRenderList, ssrRenderAttrs } from "vue/server-renderer";
import { _ as _export_sfc, a2 as useElementHover, a3 as injectionKeyIsVerticalNavHovered, K as useLayoutConfigStore, J as useRoute, M as layoutConfig, D as useRouter, L as isNavGroupActive, a4 as openGroups, a5 as useMounted, N as getDynamicI18nProps, a6 as useWindowSize, a7 as useToggle, a8 as syncRef, O as getComputedNavLinkToProp, P as isNavLinkActive, V as VIcon, S as VSpacer, t as themeConfig } from "../server.mjs";
import Footer from "./Footer-DTgAlwVo.js";
import _sfc_main$7 from "./NavbarThemeSwitcher-D7hOas_S.js";
import _sfc_main$9 from "./UserProfile-gvCVG-UJ.js";
import { c as canViewNavMenuGroup, a as can, _ as _sfc_main$8 } from "./I18n-Dk6xY8D3.js";
import { V as VNodeRenderer, _ as __nuxt_component_0 } from "./VNodeRenderer-Bw61TUGP.js";
import { PerfectScrollbar } from "vue3-perfect-scrollbar";
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
import "@casl/vue";
const _sfc_main$6 = defineComponent({
  name: "TransitionExpand",
  setup(_, { slots }) {
    const onEnter = (element) => {
      const width = getComputedStyle(element).width;
      element.style.width = width;
      element.style.position = "absolute";
      element.style.visibility = "hidden";
      element.style.height = "auto";
      const height = getComputedStyle(element).height;
      element.style.width = "";
      element.style.position = "";
      element.style.visibility = "";
      element.style.height = "0px";
      getComputedStyle(element).height;
      requestAnimationFrame(() => {
        element.style.height = height;
      });
    };
    const onAfterEnter = (element) => {
      element.style.height = "auto";
    };
    const onLeave = (element) => {
      const height = getComputedStyle(element).height;
      element.style.height = height;
      getComputedStyle(element).height;
      requestAnimationFrame(() => {
        element.style.height = "0px";
      });
    };
    return () => h(
      h(Transition),
      {
        name: "expand",
        onEnter,
        onAfterEnter,
        onLeave
      },
      () => {
        var _a;
        return (_a = slots.default) == null ? void 0 : _a.call(slots);
      }
    );
  }
});
const _sfc_setup$6 = _sfc_main$6.setup;
_sfc_main$6.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/TransitionExpand.vue");
  return _sfc_setup$6 ? _sfc_setup$6(props, ctx) : void 0;
};
const TransitionExpand = /* @__PURE__ */ _export_sfc(_sfc_main$6, [["__scopeId", "data-v-51ebc8c6"]]);
const _sfc_main$5 = /* @__PURE__ */ defineComponent({
  __name: "VerticalNav",
  __ssrInlineRender: true,
  props: {
    tag: { default: "aside" },
    navItems: {},
    isOverlayNavActive: { type: Boolean },
    toggleIsOverlayNavActive: {}
  },
  setup(__props) {
    const props = __props;
    const refNav = ref();
    const isHovered = useElementHover(refNav);
    provide(injectionKeyIsVerticalNavHovered, isHovered);
    const configStore = useLayoutConfigStore();
    const resolveNavItemComponent = (item) => {
      if ("heading" in item)
        return _sfc_main$1;
      if ("children" in item)
        return _sfc_main$4;
      return _sfc_main$2;
    };
    const route = useRoute();
    watch(() => route.name, () => {
      props.toggleIsOverlayNavActive(false);
    });
    const isVerticalNavScrolled = ref(false);
    const updateIsVerticalNavScrolled = (val) => isVerticalNavScrolled.value = val;
    const handleNavScroll = (evt) => {
      isVerticalNavScrolled.value = evt.target.scrollTop > 0;
    };
    const hideTitleAndIcon = configStore.isVerticalNavMini(isHovered);
    return (_ctx, _push, _parent, _attrs) => {
      const _component_NuxtLink = __nuxt_component_0;
      ssrRenderVNode(_push, createVNode(resolveDynamicComponent(props.tag), mergeProps({
        ref_key: "refNav",
        ref: refNav,
        class: ["layout-vertical-nav", [
          {
            "overlay-nav": unref(configStore).isLessThanOverlayNavBreakpoint,
            "hovered": unref(isHovered),
            "visible": _ctx.isOverlayNavActive,
            "scrolled": unref(isVerticalNavScrolled)
          }
        ]]
      }, _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(`<div class="nav-header" data-v-eaddb3ce${_scopeId}>`);
            ssrRenderSlot(_ctx.$slots, "nav-header", {}, () => {
              _push2(ssrRenderComponent(_component_NuxtLink, {
                to: "/",
                class: "app-logo app-title-wrapper"
              }, {
                default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(ssrRenderComponent(unref(VNodeRenderer), {
                      nodes: unref(layoutConfig).app.logo
                    }, null, _parent3, _scopeId2));
                    _push3(`<h1 style="${ssrRenderStyle(!unref(hideTitleAndIcon) ? null : { display: "none" })}" class="app-logo-title" data-v-eaddb3ce${_scopeId2}>${ssrInterpolate(unref(layoutConfig).app.title)}</h1>`);
                  } else {
                    return [
                      createVNode(unref(VNodeRenderer), {
                        nodes: unref(layoutConfig).app.logo
                      }, null, 8, ["nodes"]),
                      createVNode(Transition, { name: "vertical-nav-app-title" }, {
                        default: withCtx(() => [
                          withDirectives(createVNode("h1", { class: "app-logo-title" }, toDisplayString(unref(layoutConfig).app.title), 513), [
                            [vShow, !unref(hideTitleAndIcon)]
                          ])
                        ]),
                        _: 1
                      })
                    ];
                  }
                }),
                _: 1
              }, _parent2, _scopeId));
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({
                style: unref(configStore).isVerticalNavCollapsed ? null : { display: "none" },
                class: ["header-action d-none nav-unpin", unref(configStore).isVerticalNavCollapsed && "d-lg-block"]
              }, unref(layoutConfig).icons.verticalNavUnPinned, {
                onClick: ($event) => unref(configStore).isVerticalNavCollapsed = !unref(configStore).isVerticalNavCollapsed
              }), null), _parent2, _scopeId);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({
                style: !unref(configStore).isVerticalNavCollapsed ? null : { display: "none" },
                class: ["header-action d-none nav-pin", !unref(configStore).isVerticalNavCollapsed && "d-lg-block"]
              }, unref(layoutConfig).icons.verticalNavPinned, {
                onClick: ($event) => unref(configStore).isVerticalNavCollapsed = !unref(configStore).isVerticalNavCollapsed
              }), null), _parent2, _scopeId);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({ class: "header-action d-lg-none" }, unref(layoutConfig).icons.close, {
                onClick: ($event) => _ctx.toggleIsOverlayNavActive(false)
              }), null), _parent2, _scopeId);
            }, _push2, _parent2, _scopeId);
            _push2(`</div>`);
            ssrRenderSlot(_ctx.$slots, "before-nav-items", {}, () => {
              _push2(`<div class="vertical-nav-items-shadow" data-v-eaddb3ce${_scopeId}></div>`);
            }, _push2, _parent2, _scopeId);
            ssrRenderSlot(_ctx.$slots, "nav-items", { updateIsVerticalNavScrolled }, () => {
              _push2(ssrRenderComponent(unref(PerfectScrollbar), {
                key: unref(configStore).isAppRTL,
                tag: "ul",
                class: "nav-items",
                options: { wheelPropagation: false },
                onPsScrollY: handleNavScroll
              }, {
                default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(`<!--[-->`);
                    ssrRenderList(_ctx.navItems, (item, index) => {
                      ssrRenderVNode(_push3, createVNode(resolveDynamicComponent(resolveNavItemComponent(item)), {
                        key: index,
                        item
                      }, null), _parent3, _scopeId2);
                    });
                    _push3(`<!--]-->`);
                  } else {
                    return [
                      (openBlock(true), createBlock(Fragment, null, renderList(_ctx.navItems, (item, index) => {
                        return openBlock(), createBlock(resolveDynamicComponent(resolveNavItemComponent(item)), {
                          key: index,
                          item
                        }, null, 8, ["item"]);
                      }), 128))
                    ];
                  }
                }),
                _: 1
              }, _parent2, _scopeId));
            }, _push2, _parent2, _scopeId);
          } else {
            return [
              createVNode("div", { class: "nav-header" }, [
                renderSlot(_ctx.$slots, "nav-header", {}, () => [
                  createVNode(_component_NuxtLink, {
                    to: "/",
                    class: "app-logo app-title-wrapper"
                  }, {
                    default: withCtx(() => [
                      createVNode(unref(VNodeRenderer), {
                        nodes: unref(layoutConfig).app.logo
                      }, null, 8, ["nodes"]),
                      createVNode(Transition, { name: "vertical-nav-app-title" }, {
                        default: withCtx(() => [
                          withDirectives(createVNode("h1", { class: "app-logo-title" }, toDisplayString(unref(layoutConfig).app.title), 513), [
                            [vShow, !unref(hideTitleAndIcon)]
                          ])
                        ]),
                        _: 1
                      })
                    ]),
                    _: 1
                  }),
                  withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({
                    class: ["header-action d-none nav-unpin", unref(configStore).isVerticalNavCollapsed && "d-lg-block"]
                  }, unref(layoutConfig).icons.verticalNavUnPinned, {
                    onClick: ($event) => unref(configStore).isVerticalNavCollapsed = !unref(configStore).isVerticalNavCollapsed
                  }), null, 16, ["class", "onClick"])), [
                    [vShow, unref(configStore).isVerticalNavCollapsed]
                  ]),
                  withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({
                    class: ["header-action d-none nav-pin", !unref(configStore).isVerticalNavCollapsed && "d-lg-block"]
                  }, unref(layoutConfig).icons.verticalNavPinned, {
                    onClick: ($event) => unref(configStore).isVerticalNavCollapsed = !unref(configStore).isVerticalNavCollapsed
                  }), null, 16, ["class", "onClick"])), [
                    [vShow, !unref(configStore).isVerticalNavCollapsed]
                  ]),
                  (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({ class: "header-action d-lg-none" }, unref(layoutConfig).icons.close, {
                    onClick: ($event) => _ctx.toggleIsOverlayNavActive(false)
                  }), null, 16, ["onClick"]))
                ], true)
              ]),
              renderSlot(_ctx.$slots, "before-nav-items", {}, () => [
                createVNode("div", { class: "vertical-nav-items-shadow" })
              ], true),
              renderSlot(_ctx.$slots, "nav-items", { updateIsVerticalNavScrolled }, () => [
                (openBlock(), createBlock(unref(PerfectScrollbar), {
                  key: unref(configStore).isAppRTL,
                  tag: "ul",
                  class: "nav-items",
                  options: { wheelPropagation: false },
                  onPsScrollY: handleNavScroll
                }, {
                  default: withCtx(() => [
                    (openBlock(true), createBlock(Fragment, null, renderList(_ctx.navItems, (item, index) => {
                      return openBlock(), createBlock(resolveDynamicComponent(resolveNavItemComponent(item)), {
                        key: index,
                        item
                      }, null, 8, ["item"]);
                    }), 128))
                  ]),
                  _: 1
                }))
              ], true)
            ];
          }
        }),
        _: 3
      }), _parent);
    };
  }
});
const _sfc_setup$5 = _sfc_main$5.setup;
_sfc_main$5.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/VerticalNav.vue");
  return _sfc_setup$5 ? _sfc_setup$5(props, ctx) : void 0;
};
const VerticalNav = /* @__PURE__ */ _export_sfc(_sfc_main$5, [["__scopeId", "data-v-eaddb3ce"]]);
const _sfc_main$4 = /* @__PURE__ */ defineComponent({
  ...{
    name: "VerticalNavGroup"
  },
  __name: "VerticalNavGroup",
  __ssrInlineRender: true,
  props: {
    item: {}
  },
  setup(__props) {
    const props = __props;
    const route = useRoute();
    const router = useRouter();
    const configStore = useLayoutConfigStore();
    const hideTitleAndBadge = configStore.isVerticalNavMini();
    const isVerticalNavHovered = inject(injectionKeyIsVerticalNavHovered, ref(false));
    const isGroupActive = ref(false);
    const isGroupOpen = ref(false);
    const isAnyChildOpen = (children) => {
      return children.some((child) => {
        let result = openGroups.value.includes(child.title);
        if ("children" in child)
          result = isAnyChildOpen(child.children) || result;
        return result;
      });
    };
    const collapseChildren = (children) => {
      children.forEach((child) => {
        if ("children" in child)
          collapseChildren(child.children);
        openGroups.value = openGroups.value.filter((group) => group !== child.title);
      });
    };
    watch(() => route.path, () => {
      const isActive = isNavGroupActive(props.item.children, router);
      isGroupOpen.value = isActive && !configStore.isVerticalNavMini(isVerticalNavHovered).value;
      isGroupActive.value = isActive;
    }, { immediate: true });
    watch(isGroupOpen, (val) => {
      const grpIndex = openGroups.value.indexOf(props.item.title);
      if (val && grpIndex === -1) {
        openGroups.value.push(props.item.title);
      } else if (!val && grpIndex !== -1) {
        openGroups.value.splice(grpIndex, 1);
        collapseChildren(props.item.children);
      }
    }, { immediate: true });
    watch(openGroups, (val) => {
      const lastOpenedGroup = val.at(-1);
      if (lastOpenedGroup === props.item.title)
        return;
      const isActive = isNavGroupActive(props.item.children, router);
      if (isActive)
        return;
      if (isAnyChildOpen(props.item.children))
        return;
      isGroupOpen.value = isActive;
      isGroupActive.value = isActive;
    }, { deep: true });
    watch(
      configStore.isVerticalNavMini(isVerticalNavHovered),
      (val) => {
        isGroupOpen.value = val ? false : isGroupActive.value;
      }
    );
    const isMounted = useMounted();
    return (_ctx, _push, _parent, _attrs) => {
      if (unref(canViewNavMenuGroup)(_ctx.item)) {
        _push(`<li${ssrRenderAttrs(mergeProps({
          class: ["nav-group", [
            {
              active: unref(isGroupActive),
              open: unref(isGroupOpen),
              disabled: _ctx.item.disable
            }
          ]]
        }, _attrs))}><div class="nav-group-label">`);
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps(_ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps, { class: "nav-item-icon" }), null), _parent);
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(unref(isMounted) ? TransitionGroup : "div"), mergeProps({ name: "transition-slide-x" }, !unref(isMounted) ? { class: "d-flex align-center flex-grow-1" } : void 0), {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps(unref(getDynamicI18nProps)(_ctx.item.title, "span"), {
                style: !unref(hideTitleAndBadge) ? null : { display: "none" },
                key: "title",
                class: "nav-item-title"
              }), {
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
              if (_ctx.item.badgeContent) {
                ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps(unref(getDynamicI18nProps)(_ctx.item.badgeContent, "span"), {
                  style: !unref(hideTitleAndBadge) ? null : { display: "none" },
                  key: "badge",
                  class: ["nav-item-badge", _ctx.item.badgeClass]
                }), {
                  default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                    if (_push3) {
                      _push3(`${ssrInterpolate(_ctx.item.badgeContent)}`);
                    } else {
                      return [
                        createTextVNode(toDisplayString(_ctx.item.badgeContent), 1)
                      ];
                    }
                  }),
                  _: 1
                }), _parent2, _scopeId);
              } else {
                _push2(`<!---->`);
              }
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps({
                style: !unref(hideTitleAndBadge) ? null : { display: "none" }
              }, unref(layoutConfig).icons.chevronRight, {
                key: "arrow",
                class: "nav-group-arrow"
              }), null), _parent2, _scopeId);
            } else {
              return [
                withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps(unref(getDynamicI18nProps)(_ctx.item.title, "span"), {
                  key: "title",
                  class: "nav-item-title"
                }), {
                  default: withCtx(() => [
                    createTextVNode(toDisplayString(_ctx.item.title), 1)
                  ]),
                  _: 1
                }, 16)), [
                  [vShow, !unref(hideTitleAndBadge)]
                ]),
                _ctx.item.badgeContent ? withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({ key: 0 }, unref(getDynamicI18nProps)(_ctx.item.badgeContent, "span"), {
                  key: "badge",
                  class: ["nav-item-badge", _ctx.item.badgeClass]
                }), {
                  default: withCtx(() => [
                    createTextVNode(toDisplayString(_ctx.item.badgeContent), 1)
                  ]),
                  _: 1
                }, 16, ["class"])), [
                  [vShow, !unref(hideTitleAndBadge)]
                ]) : createCommentVNode("", true),
                withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps(unref(layoutConfig).icons.chevronRight, {
                  key: "arrow",
                  class: "nav-group-arrow"
                }), null, 16)), [
                  [vShow, !unref(hideTitleAndBadge)]
                ])
              ];
            }
          }),
          _: 1
        }), _parent);
        _push(`</div>`);
        _push(ssrRenderComponent(unref(TransitionExpand), null, {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              _push2(`<ul style="${ssrRenderStyle(unref(isGroupOpen) ? null : { display: "none" })}" class="nav-group-children"${_scopeId}><!--[-->`);
              ssrRenderList(_ctx.item.children, (child) => {
                ssrRenderVNode(_push2, createVNode(resolveDynamicComponent("children" in child ? "VerticalNavGroup" : unref(_sfc_main$2)), {
                  key: child.title,
                  item: child
                }, null), _parent2, _scopeId);
              });
              _push2(`<!--]--></ul>`);
            } else {
              return [
                withDirectives(createVNode("ul", { class: "nav-group-children" }, [
                  (openBlock(true), createBlock(Fragment, null, renderList(_ctx.item.children, (child) => {
                    return openBlock(), createBlock(resolveDynamicComponent("children" in child ? "VerticalNavGroup" : unref(_sfc_main$2)), {
                      key: child.title,
                      item: child
                    }, null, 8, ["item"]);
                  }), 128))
                ], 512), [
                  [vShow, unref(isGroupOpen)]
                ])
              ];
            }
          }),
          _: 1
        }, _parent));
        _push(`</li>`);
      } else {
        _push(`<!---->`);
      }
    };
  }
});
const _sfc_setup$4 = _sfc_main$4.setup;
_sfc_main$4.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/VerticalNavGroup.vue");
  return _sfc_setup$4 ? _sfc_setup$4(props, ctx) : void 0;
};
const _sfc_main$3 = defineComponent({
  props: {
    navItems: {
      type: Array,
      required: true
    },
    verticalNavAttrs: {
      type: Object,
      default: () => ({})
    }
  },
  setup(props, { slots }) {
    const { width: windowWidth } = useWindowSize();
    const configStore = useLayoutConfigStore();
    const isOverlayNavActive = ref(false);
    const isLayoutOverlayVisible = ref(false);
    const toggleIsOverlayNavActive = useToggle(isOverlayNavActive);
    syncRef(isOverlayNavActive, isLayoutOverlayVisible);
    watch(windowWidth, () => {
      if (!configStore.isLessThanOverlayNavBreakpoint && isLayoutOverlayVisible.value)
        isLayoutOverlayVisible.value = false;
    });
    return () => {
      var _a, _b, _c;
      const verticalNavAttrs = toRef(props, "verticalNavAttrs");
      const { wrapper: verticalNavWrapper, wrapperProps: verticalNavWrapperProps, ...additionalVerticalNavAttrs } = verticalNavAttrs.value;
      const verticalNav = h(
        VerticalNav,
        { isOverlayNavActive: isOverlayNavActive.value, toggleIsOverlayNavActive, navItems: props.navItems, ...additionalVerticalNavAttrs },
        {
          "nav-header": () => {
            var _a2;
            return (_a2 = slots["vertical-nav-header"]) == null ? void 0 : _a2.call(slots);
          },
          "before-nav-items": () => {
            var _a2;
            return (_a2 = slots["before-vertical-nav-items"]) == null ? void 0 : _a2.call(slots);
          }
        }
      );
      const navbar = h(
        "header",
        { class: ["layout-navbar", { "navbar-blur": configStore.isNavbarBlurEnabled }] },
        [
          h(
            "div",
            { class: "navbar-content-container" },
            (_a = slots.navbar) == null ? void 0 : _a.call(slots, {
              toggleVerticalOverlayNavActive: toggleIsOverlayNavActive
            })
          )
        ]
      );
      const main = h(
        "main",
        { class: "layout-page-content" },
        h("div", { class: "page-content-container" }, (_b = slots.default) == null ? void 0 : _b.call(slots))
      );
      const footer = h(
        "footer",
        { class: "layout-footer" },
        [
          h(
            "div",
            { class: "footer-content-container" },
            (_c = slots.footer) == null ? void 0 : _c.call(slots)
          )
        ]
      );
      const layoutOverlay = h(
        "div",
        {
          class: ["layout-overlay", { visible: isLayoutOverlayVisible.value }],
          onClick: () => {
            isLayoutOverlayVisible.value = !isLayoutOverlayVisible.value;
          }
        }
      );
      return h(
        "div",
        { class: ["layout-wrapper", ...configStore._layoutClasses] },
        [
          verticalNavWrapper ? h(verticalNavWrapper, verticalNavWrapperProps, { default: () => verticalNav }) : verticalNav,
          h(
            "div",
            { class: "layout-content-wrapper" },
            [
              navbar,
              main,
              footer
            ]
          ),
          layoutOverlay
        ]
      );
    };
  }
});
const _sfc_setup$3 = _sfc_main$3.setup;
_sfc_main$3.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/VerticalNavLayout.vue");
  return _sfc_setup$3 ? _sfc_setup$3(props, ctx) : void 0;
};
const _sfc_main$2 = /* @__PURE__ */ defineComponent({
  __name: "VerticalNavLink",
  __ssrInlineRender: true,
  props: {
    item: {}
  },
  setup(__props) {
    const configStore = useLayoutConfigStore();
    const hideTitleAndBadge = configStore.isVerticalNavMini();
    return (_ctx, _push, _parent, _attrs) => {
      if (unref(can)(_ctx.item.action, _ctx.item.subject)) {
        _push(`<li${ssrRenderAttrs(mergeProps({
          class: ["nav-link", { disabled: _ctx.item.disable }]
        }, _attrs))}>`);
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(_ctx.item.to ? unref(__nuxt_component_0) : "a"), mergeProps(unref(getComputedNavLinkToProp)(_ctx.item), {
          class: { "router-link-active router-link-exact-active": unref(isNavLinkActive)(_ctx.item, _ctx.$router) }
        }), {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps(_ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps, { class: "nav-item-icon" }), null), _parent2, _scopeId);
              _push2(`<!--[-->`);
              ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({
                style: !unref(hideTitleAndBadge) ? null : { display: "none" },
                key: "title",
                class: "nav-item-title"
              }, unref(getDynamicI18nProps)(_ctx.item.title, "span")), {
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
              if (_ctx.item.badgeContent) {
                ssrRenderVNode(_push2, createVNode(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({
                  style: !unref(hideTitleAndBadge) ? null : { display: "none" },
                  key: "badge",
                  class: ["nav-item-badge", _ctx.item.badgeClass]
                }, unref(getDynamicI18nProps)(_ctx.item.badgeContent, "span")), {
                  default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                    if (_push3) {
                      _push3(`${ssrInterpolate(_ctx.item.badgeContent)}`);
                    } else {
                      return [
                        createTextVNode(toDisplayString(_ctx.item.badgeContent), 1)
                      ];
                    }
                  }),
                  _: 1
                }), _parent2, _scopeId);
              } else {
                _push2(`<!---->`);
              }
              _push2(`<!--]-->`);
            } else {
              return [
                (openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.iconRenderer || "div"), mergeProps(_ctx.item.icon || unref(layoutConfig).verticalNav.defaultNavItemIconProps, { class: "nav-item-icon" }), null, 16)),
                createVNode(TransitionGroup, { name: "transition-slide-x" }, {
                  default: withCtx(() => [
                    withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({
                      key: "title",
                      class: "nav-item-title"
                    }, unref(getDynamicI18nProps)(_ctx.item.title, "span")), {
                      default: withCtx(() => [
                        createTextVNode(toDisplayString(_ctx.item.title), 1)
                      ]),
                      _: 1
                    }, 16)), [
                      [vShow, !unref(hideTitleAndBadge)]
                    ]),
                    _ctx.item.badgeContent ? withDirectives((openBlock(), createBlock(resolveDynamicComponent(unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({
                      key: "badge",
                      class: ["nav-item-badge", _ctx.item.badgeClass]
                    }, unref(getDynamicI18nProps)(_ctx.item.badgeContent, "span")), {
                      default: withCtx(() => [
                        createTextVNode(toDisplayString(_ctx.item.badgeContent), 1)
                      ]),
                      _: 1
                    }, 16, ["class"])), [
                      [vShow, !unref(hideTitleAndBadge)]
                    ]) : createCommentVNode("", true)
                  ]),
                  _: 1
                })
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
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/VerticalNavLink.vue");
  return _sfc_setup$2 ? _sfc_setup$2(props, ctx) : void 0;
};
const _sfc_main$1 = /* @__PURE__ */ defineComponent({
  __name: "VerticalNavSectionTitle",
  __ssrInlineRender: true,
  props: {
    item: {}
  },
  setup(__props) {
    const configStore = useLayoutConfigStore();
    const shallRenderIcon = configStore.isVerticalNavMini();
    return (_ctx, _push, _parent, _attrs) => {
      if (unref(can)(_ctx.item.action, _ctx.item.subject)) {
        _push(`<li${ssrRenderAttrs(mergeProps({ class: "nav-section-title" }, _attrs))}><div class="title-wrapper">`);
        ssrRenderVNode(_push, createVNode(resolveDynamicComponent(unref(shallRenderIcon) ? unref(layoutConfig).app.iconRenderer : unref(layoutConfig).app.i18n.enable ? "i18n-t" : "span"), mergeProps({
          key: unref(shallRenderIcon),
          class: unref(shallRenderIcon) ? "placeholder-icon" : "title-text"
        }, { ...unref(layoutConfig).icons.sectionTitlePlaceholder, ...unref(getDynamicI18nProps)(_ctx.item.heading, "span") }), {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              _push2(`${ssrInterpolate(!unref(shallRenderIcon) ? _ctx.item.heading : null)}`);
            } else {
              return [
                createTextVNode(toDisplayString(!unref(shallRenderIcon) ? _ctx.item.heading : null), 1)
              ];
            }
          }),
          _: 1
        }), _parent);
        _push(`</div></li>`);
      } else {
        _push(`<!---->`);
      }
    };
  }
});
const _sfc_setup$1 = _sfc_main$1.setup;
_sfc_main$1.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@layouts/components/VerticalNavSectionTitle.vue");
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
  __name: "DefaultLayoutWithVerticalNav",
  __ssrInlineRender: true,
  setup(__props) {
    return (_ctx, _push, _parent, _attrs) => {
      const _component_IconBtn = resolveComponent("IconBtn");
      _push(ssrRenderComponent(unref(_sfc_main$3), mergeProps({ "nav-items": unref(navItems) }, _attrs), {
        navbar: withCtx(({ toggleVerticalOverlayNavActive }, _push2, _parent2, _scopeId) => {
          var _a, _b;
          if (_push2) {
            _push2(`<div class="d-flex h-100 align-center"${_scopeId}>`);
            _push2(ssrRenderComponent(_component_IconBtn, {
              id: "vertical-nav-toggle-btn",
              class: "ms-n3 d-lg-none",
              onClick: ($event) => toggleVerticalOverlayNavActive(true)
            }, {
              default: withCtx((_, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VIcon, {
                    size: "26",
                    icon: "tabler-menu-2"
                  }, null, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VIcon, {
                      size: "26",
                      icon: "tabler-menu-2"
                    })
                  ];
                }
              }),
              _: 2
            }, _parent2, _scopeId));
            _push2(ssrRenderComponent(_sfc_main$7, null, null, _parent2, _scopeId));
            _push2(ssrRenderComponent(VSpacer, null, null, _parent2, _scopeId));
            if (unref(themeConfig).app.i18n.enable && ((_a = unref(themeConfig).app.i18n.langConfig) == null ? void 0 : _a.length)) {
              _push2(ssrRenderComponent(_sfc_main$8, {
                languages: unref(themeConfig).app.i18n.langConfig
              }, null, _parent2, _scopeId));
            } else {
              _push2(`<!---->`);
            }
            _push2(ssrRenderComponent(_sfc_main$9, null, null, _parent2, _scopeId));
            _push2(`</div>`);
          } else {
            return [
              createVNode("div", { class: "d-flex h-100 align-center" }, [
                createVNode(_component_IconBtn, {
                  id: "vertical-nav-toggle-btn",
                  class: "ms-n3 d-lg-none",
                  onClick: ($event) => toggleVerticalOverlayNavActive(true)
                }, {
                  default: withCtx(() => [
                    createVNode(VIcon, {
                      size: "26",
                      icon: "tabler-menu-2"
                    })
                  ]),
                  _: 2
                }, 1032, ["onClick"]),
                createVNode(_sfc_main$7),
                createVNode(VSpacer),
                unref(themeConfig).app.i18n.enable && ((_b = unref(themeConfig).app.i18n.langConfig) == null ? void 0 : _b.length) ? (openBlock(), createBlock(_sfc_main$8, {
                  key: 0,
                  languages: unref(themeConfig).app.i18n.langConfig
                }, null, 8, ["languages"])) : createCommentVNode("", true),
                createVNode(_sfc_main$9)
              ])
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
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/components/DefaultLayoutWithVerticalNav.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
