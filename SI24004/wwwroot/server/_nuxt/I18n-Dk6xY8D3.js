import { getCurrentInstance, defineComponent, resolveComponent, withCtx, unref, createTextVNode, toDisplayString, createVNode, openBlock, createBlock, Fragment, renderList, useSSRContext } from "vue";
import "@casl/vue";
import { ssrRenderComponent, ssrRenderList, ssrInterpolate } from "vue/server-renderer";
import { V as VIcon, U as VMenu, W as VList, X as VListItem, a0 as VListItemTitle } from "../server.mjs";
const can = (action, subject) => {
  var _a;
  const vm = getCurrentInstance();
  if (!vm)
    return false;
  const localCan = vm.proxy && "$can" in vm.proxy;
  return localCan ? (_a = vm.proxy) == null ? void 0 : _a.$can(action, subject) : true;
};
const canViewNavMenuGroup = (item) => {
  const hasAnyVisibleChild = item.children.some((i) => can(i.action, i.subject));
  if (!(item.action && item.subject))
    return hasAnyVisibleChild;
  return can(item.action, item.subject) && hasAnyVisibleChild;
};
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "I18n",
  __ssrInlineRender: true,
  props: {
    languages: {},
    location: { default: "bottom end" }
  },
  setup(__props) {
    const props = __props;
    const { locale } = useI18n({ useScope: "global" });
    return (_ctx, _push, _parent, _attrs) => {
      const _component_IconBtn = resolveComponent("IconBtn");
      _push(ssrRenderComponent(_component_IconBtn, _attrs, {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VIcon, {
              size: "24",
              icon: "tabler-language"
            }, null, _parent2, _scopeId));
            _push2(ssrRenderComponent(VMenu, {
              activator: "parent",
              location: props.location,
              offset: "12px"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VList, {
                    selected: [unref(locale)],
                    color: "primary",
                    "min-width": "175px"
                  }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(`<!--[-->`);
                        ssrRenderList(props.languages, (lang) => {
                          _push4(ssrRenderComponent(VListItem, {
                            key: lang.i18nLang,
                            value: lang.i18nLang,
                            onClick: ($event) => locale.value = lang.i18nLang
                          }, {
                            default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                              if (_push5) {
                                _push5(ssrRenderComponent(VListItemTitle, null, {
                                  default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                    if (_push6) {
                                      _push6(`${ssrInterpolate(lang.label)}`);
                                    } else {
                                      return [
                                        createTextVNode(toDisplayString(lang.label), 1)
                                      ];
                                    }
                                  }),
                                  _: 2
                                }, _parent5, _scopeId4));
                              } else {
                                return [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode(toDisplayString(lang.label), 1)
                                    ]),
                                    _: 2
                                  }, 1024)
                                ];
                              }
                            }),
                            _: 2
                          }, _parent4, _scopeId3));
                        });
                        _push4(`<!--]-->`);
                      } else {
                        return [
                          (openBlock(true), createBlock(Fragment, null, renderList(props.languages, (lang) => {
                            return openBlock(), createBlock(VListItem, {
                              key: lang.i18nLang,
                              value: lang.i18nLang,
                              onClick: ($event) => locale.value = lang.i18nLang
                            }, {
                              default: withCtx(() => [
                                createVNode(VListItemTitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode(toDisplayString(lang.label), 1)
                                  ]),
                                  _: 2
                                }, 1024)
                              ]),
                              _: 2
                            }, 1032, ["value", "onClick"]);
                          }), 128))
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VList, {
                      selected: [unref(locale)],
                      color: "primary",
                      "min-width": "175px"
                    }, {
                      default: withCtx(() => [
                        (openBlock(true), createBlock(Fragment, null, renderList(props.languages, (lang) => {
                          return openBlock(), createBlock(VListItem, {
                            key: lang.i18nLang,
                            value: lang.i18nLang,
                            onClick: ($event) => locale.value = lang.i18nLang
                          }, {
                            default: withCtx(() => [
                              createVNode(VListItemTitle, null, {
                                default: withCtx(() => [
                                  createTextVNode(toDisplayString(lang.label), 1)
                                ]),
                                _: 2
                              }, 1024)
                            ]),
                            _: 2
                          }, 1032, ["value", "onClick"]);
                        }), 128))
                      ]),
                      _: 1
                    }, 8, ["selected"])
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
          } else {
            return [
              createVNode(VIcon, {
                size: "24",
                icon: "tabler-language"
              }),
              createVNode(VMenu, {
                activator: "parent",
                location: props.location,
                offset: "12px"
              }, {
                default: withCtx(() => [
                  createVNode(VList, {
                    selected: [unref(locale)],
                    color: "primary",
                    "min-width": "175px"
                  }, {
                    default: withCtx(() => [
                      (openBlock(true), createBlock(Fragment, null, renderList(props.languages, (lang) => {
                        return openBlock(), createBlock(VListItem, {
                          key: lang.i18nLang,
                          value: lang.i18nLang,
                          onClick: ($event) => locale.value = lang.i18nLang
                        }, {
                          default: withCtx(() => [
                            createVNode(VListItemTitle, null, {
                              default: withCtx(() => [
                                createTextVNode(toDisplayString(lang.label), 1)
                              ]),
                              _: 2
                            }, 1024)
                          ]),
                          _: 2
                        }, 1032, ["value", "onClick"]);
                      }), 128))
                    ]),
                    _: 1
                  }, 8, ["selected"])
                ]),
                _: 1
              }, 8, ["location"])
            ];
          }
        }),
        _: 1
      }, _parent));
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@core/components/I18n.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as _,
  can as a,
  canViewNavMenuGroup as c
};
