import { defineComponent, ref, watch, defineAsyncComponent, resolveComponent, mergeProps, unref, withCtx, createVNode, isRef, createTextVNode, toDisplayString, openBlock, createBlock, Fragment, renderList, createCommentVNode, useSSRContext } from "vue";
import { G as useConfigStore, D as useRouter, V as VIcon, d as VCardText, f as VRow, g as VCol, W as VList, X as VListItem, a0 as VListItemTitle, aa as VListSubheader } from "../server.mjs";
import { a as useApi } from "./useApi-DkwIRz4i.js";
import { ssrRenderAttrs, ssrRenderComponent, ssrRenderList, ssrRenderStyle, ssrInterpolate } from "vue/server-renderer";
import Shepherd from "shepherd.js";
import { withQuery } from "ufo";
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
const _sfc_main = /* @__PURE__ */ defineComponent({
  ...{
    inheritAttrs: false
  },
  __name: "NavSearchBar",
  __ssrInlineRender: true,
  setup(__props) {
    const configStore = useConfigStore();
    const isAppSearchBarVisible = ref(false);
    const isLoading = ref(false);
    const suggestionGroups = [
      {
        title: "Popular Searches",
        content: [
          { icon: "tabler-chart-bar", title: "Analytics", url: { name: "dashboards-analytics" } },
          { icon: "tabler-chart-donut-3", title: "CRM", url: { name: "dashboards-crm" } },
          { icon: "tabler-shopping-cart", title: "eCommerce", url: { name: "dashboards-ecommerce" } },
          { icon: "tabler-truck", title: "Logistics", url: { name: "dashboards-logistics" } }
        ]
      },
      {
        title: "Apps & Pages",
        content: [
          { icon: "tabler-calendar", title: "Calendar", url: { name: "apps-calendar" } },
          { icon: "tabler-lock", title: "Roles & Permissions", url: { name: "apps-roles" } },
          { icon: "tabler-settings", title: "Account Settings", url: { name: "pages-account-settings-tab", params: { tab: "account" } } },
          { icon: "tabler-copy", title: "Dialog Examples", url: { name: "pages-dialog-examples" } }
        ]
      },
      {
        title: "User Interface",
        content: [
          { icon: "tabler-typography", title: "Typography", url: { name: "pages-typography" } },
          { icon: "tabler-menu-2", title: "Accordion", url: { name: "components-expansion-panel" } },
          { icon: "tabler-info-triangle", title: "Alert", url: { name: "components-alert" } },
          { icon: "tabler-checkbox", title: "Cards", url: { name: "pages-cards-card-basic" } }
        ]
      },
      {
        title: "Forms & Tables",
        content: [
          { icon: "tabler-circle-dot", title: "Radio", url: { name: "forms-radio" } },
          { icon: "tabler-file-invoice", title: "Form Layouts", url: { name: "forms-form-layouts" } },
          { icon: "tabler-table", title: "Table", url: { name: "tables-data-table" } },
          { icon: "tabler-edit", title: "Editor", url: { name: "forms-editors" } }
        ]
      }
    ];
    const noDataSuggestions = [
      {
        title: "Analytics",
        icon: "tabler-chart-bar",
        url: { name: "dashboards-analytics" }
      },
      {
        title: "CRM",
        icon: "tabler-chart-donut-3",
        url: { name: "dashboards-crm" }
      },
      {
        title: "eCommerce",
        icon: "tabler-shopping-cart",
        url: { name: "dashboards-ecommerce" }
      }
    ];
    const searchQuery = ref("");
    const router = useRouter();
    const searchResult = ref([]);
    const fetchResults = async () => {
      isLoading.value = true;
      const { data } = await useApi(withQuery("/app-bar/search", { q: searchQuery.value }));
      searchResult.value = data.value;
      setTimeout(() => {
        isLoading.value = false;
      }, 500);
    };
    watch(searchQuery, fetchResults);
    const closeSearchBar = () => {
      isAppSearchBarVisible.value = false;
      searchQuery.value = "";
    };
    const redirectToSuggestedPage = (selected) => {
      router.push(selected.url);
      closeSearchBar();
    };
    const LazyAppBarSearch = defineAsyncComponent(() => import("./AppBarSearch-Do6V9W1c.js"));
    return (_ctx, _push, _parent, _attrs) => {
      const _component_IconBtn = resolveComponent("IconBtn");
      _push(`<!--[--><div${ssrRenderAttrs(mergeProps({ class: "d-flex align-center cursor-pointer" }, _ctx.$attrs, { style: { "user-select": "none" } }))}>`);
      _push(ssrRenderComponent(_component_IconBtn, {
        onClick: ($event) => {
          var _a;
          return (_a = unref(Shepherd).activeTour) == null ? void 0 : _a.cancel();
        }
      }, {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VIcon, { icon: "tabler-search" }, null, _parent2, _scopeId));
          } else {
            return [
              createVNode(VIcon, { icon: "tabler-search" })
            ];
          }
        }),
        _: 1
      }, _parent));
      if (unref(configStore).appContentLayoutNav === "vertical") {
        _push(`<span class="d-none d-md-flex align-center text-disabled ms-2"><span class="me-2">Search</span><span class="meta-key">⌘K</span></span>`);
      } else {
        _push(`<!---->`);
      }
      _push(`</div>`);
      _push(ssrRenderComponent(unref(LazyAppBarSearch), {
        isDialogVisible: unref(isAppSearchBarVisible),
        "onUpdate:isDialogVisible": ($event) => isRef(isAppSearchBarVisible) ? isAppSearchBarVisible.value = $event : null,
        "search-results": unref(searchResult),
        "is-loading": unref(isLoading),
        onSearch: ($event) => searchQuery.value = $event
      }, {
        suggestions: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VCardText, { class: "app-bar-search-suggestions pa-12" }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  if (suggestionGroups) {
                    _push3(ssrRenderComponent(VRow, null, {
                      default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                        if (_push4) {
                          _push4(`<!--[-->`);
                          ssrRenderList(suggestionGroups, (suggestion) => {
                            _push4(ssrRenderComponent(VCol, {
                              key: suggestion.title,
                              cols: "12",
                              sm: "6"
                            }, {
                              default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                                if (_push5) {
                                  _push5(`<p class="custom-letter-spacing text-disabled text-uppercase py-2 px-4 mb-0" style="${ssrRenderStyle({ "font-size": "0.75rem", "line-height": "0.875rem" })}"${_scopeId4}>${ssrInterpolate(suggestion.title)}</p>`);
                                  _push5(ssrRenderComponent(VList, { class: "card-list" }, {
                                    default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                      if (_push6) {
                                        _push6(`<!--[-->`);
                                        ssrRenderList(suggestion.content, (item) => {
                                          _push6(ssrRenderComponent(VListItem, {
                                            key: item.title,
                                            class: "app-bar-search-suggestion mx-4 mt-2",
                                            onClick: ($event) => redirectToSuggestedPage(item)
                                          }, {
                                            prepend: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                              if (_push7) {
                                                _push7(ssrRenderComponent(VIcon, {
                                                  icon: item.icon,
                                                  size: "20",
                                                  class: "me-n1"
                                                }, null, _parent7, _scopeId6));
                                              } else {
                                                return [
                                                  createVNode(VIcon, {
                                                    icon: item.icon,
                                                    size: "20",
                                                    class: "me-n1"
                                                  }, null, 8, ["icon"])
                                                ];
                                              }
                                            }),
                                            default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                              if (_push7) {
                                                _push7(ssrRenderComponent(VListItemTitle, null, {
                                                  default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                                    if (_push8) {
                                                      _push8(`${ssrInterpolate(item.title)}`);
                                                    } else {
                                                      return [
                                                        createTextVNode(toDisplayString(item.title), 1)
                                                      ];
                                                    }
                                                  }),
                                                  _: 2
                                                }, _parent7, _scopeId6));
                                              } else {
                                                return [
                                                  createVNode(VListItemTitle, null, {
                                                    default: withCtx(() => [
                                                      createTextVNode(toDisplayString(item.title), 1)
                                                    ]),
                                                    _: 2
                                                  }, 1024)
                                                ];
                                              }
                                            }),
                                            _: 2
                                          }, _parent6, _scopeId5));
                                        });
                                        _push6(`<!--]-->`);
                                      } else {
                                        return [
                                          (openBlock(true), createBlock(Fragment, null, renderList(suggestion.content, (item) => {
                                            return openBlock(), createBlock(VListItem, {
                                              key: item.title,
                                              class: "app-bar-search-suggestion mx-4 mt-2",
                                              onClick: ($event) => redirectToSuggestedPage(item)
                                            }, {
                                              prepend: withCtx(() => [
                                                createVNode(VIcon, {
                                                  icon: item.icon,
                                                  size: "20",
                                                  class: "me-n1"
                                                }, null, 8, ["icon"])
                                              ]),
                                              default: withCtx(() => [
                                                createVNode(VListItemTitle, null, {
                                                  default: withCtx(() => [
                                                    createTextVNode(toDisplayString(item.title), 1)
                                                  ]),
                                                  _: 2
                                                }, 1024)
                                              ]),
                                              _: 2
                                            }, 1032, ["onClick"]);
                                          }), 128))
                                        ];
                                      }
                                    }),
                                    _: 2
                                  }, _parent5, _scopeId4));
                                } else {
                                  return [
                                    createVNode("p", {
                                      class: "custom-letter-spacing text-disabled text-uppercase py-2 px-4 mb-0",
                                      style: { "font-size": "0.75rem", "line-height": "0.875rem" }
                                    }, toDisplayString(suggestion.title), 1),
                                    createVNode(VList, { class: "card-list" }, {
                                      default: withCtx(() => [
                                        (openBlock(true), createBlock(Fragment, null, renderList(suggestion.content, (item) => {
                                          return openBlock(), createBlock(VListItem, {
                                            key: item.title,
                                            class: "app-bar-search-suggestion mx-4 mt-2",
                                            onClick: ($event) => redirectToSuggestedPage(item)
                                          }, {
                                            prepend: withCtx(() => [
                                              createVNode(VIcon, {
                                                icon: item.icon,
                                                size: "20",
                                                class: "me-n1"
                                              }, null, 8, ["icon"])
                                            ]),
                                            default: withCtx(() => [
                                              createVNode(VListItemTitle, null, {
                                                default: withCtx(() => [
                                                  createTextVNode(toDisplayString(item.title), 1)
                                                ]),
                                                _: 2
                                              }, 1024)
                                            ]),
                                            _: 2
                                          }, 1032, ["onClick"]);
                                        }), 128))
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
                            (openBlock(), createBlock(Fragment, null, renderList(suggestionGroups, (suggestion) => {
                              return createVNode(VCol, {
                                key: suggestion.title,
                                cols: "12",
                                sm: "6"
                              }, {
                                default: withCtx(() => [
                                  createVNode("p", {
                                    class: "custom-letter-spacing text-disabled text-uppercase py-2 px-4 mb-0",
                                    style: { "font-size": "0.75rem", "line-height": "0.875rem" }
                                  }, toDisplayString(suggestion.title), 1),
                                  createVNode(VList, { class: "card-list" }, {
                                    default: withCtx(() => [
                                      (openBlock(true), createBlock(Fragment, null, renderList(suggestion.content, (item) => {
                                        return openBlock(), createBlock(VListItem, {
                                          key: item.title,
                                          class: "app-bar-search-suggestion mx-4 mt-2",
                                          onClick: ($event) => redirectToSuggestedPage(item)
                                        }, {
                                          prepend: withCtx(() => [
                                            createVNode(VIcon, {
                                              icon: item.icon,
                                              size: "20",
                                              class: "me-n1"
                                            }, null, 8, ["icon"])
                                          ]),
                                          default: withCtx(() => [
                                            createVNode(VListItemTitle, null, {
                                              default: withCtx(() => [
                                                createTextVNode(toDisplayString(item.title), 1)
                                              ]),
                                              _: 2
                                            }, 1024)
                                          ]),
                                          _: 2
                                        }, 1032, ["onClick"]);
                                      }), 128))
                                    ]),
                                    _: 2
                                  }, 1024)
                                ]),
                                _: 2
                              }, 1024);
                            }), 64))
                          ];
                        }
                      }),
                      _: 1
                    }, _parent3, _scopeId2));
                  } else {
                    _push3(`<!---->`);
                  }
                } else {
                  return [
                    suggestionGroups ? (openBlock(), createBlock(VRow, { key: 0 }, {
                      default: withCtx(() => [
                        (openBlock(), createBlock(Fragment, null, renderList(suggestionGroups, (suggestion) => {
                          return createVNode(VCol, {
                            key: suggestion.title,
                            cols: "12",
                            sm: "6"
                          }, {
                            default: withCtx(() => [
                              createVNode("p", {
                                class: "custom-letter-spacing text-disabled text-uppercase py-2 px-4 mb-0",
                                style: { "font-size": "0.75rem", "line-height": "0.875rem" }
                              }, toDisplayString(suggestion.title), 1),
                              createVNode(VList, { class: "card-list" }, {
                                default: withCtx(() => [
                                  (openBlock(true), createBlock(Fragment, null, renderList(suggestion.content, (item) => {
                                    return openBlock(), createBlock(VListItem, {
                                      key: item.title,
                                      class: "app-bar-search-suggestion mx-4 mt-2",
                                      onClick: ($event) => redirectToSuggestedPage(item)
                                    }, {
                                      prepend: withCtx(() => [
                                        createVNode(VIcon, {
                                          icon: item.icon,
                                          size: "20",
                                          class: "me-n1"
                                        }, null, 8, ["icon"])
                                      ]),
                                      default: withCtx(() => [
                                        createVNode(VListItemTitle, null, {
                                          default: withCtx(() => [
                                            createTextVNode(toDisplayString(item.title), 1)
                                          ]),
                                          _: 2
                                        }, 1024)
                                      ]),
                                      _: 2
                                    }, 1032, ["onClick"]);
                                  }), 128))
                                ]),
                                _: 2
                              }, 1024)
                            ]),
                            _: 2
                          }, 1024);
                        }), 64))
                      ]),
                      _: 1
                    })) : createCommentVNode("", true)
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
          } else {
            return [
              createVNode(VCardText, { class: "app-bar-search-suggestions pa-12" }, {
                default: withCtx(() => [
                  suggestionGroups ? (openBlock(), createBlock(VRow, { key: 0 }, {
                    default: withCtx(() => [
                      (openBlock(), createBlock(Fragment, null, renderList(suggestionGroups, (suggestion) => {
                        return createVNode(VCol, {
                          key: suggestion.title,
                          cols: "12",
                          sm: "6"
                        }, {
                          default: withCtx(() => [
                            createVNode("p", {
                              class: "custom-letter-spacing text-disabled text-uppercase py-2 px-4 mb-0",
                              style: { "font-size": "0.75rem", "line-height": "0.875rem" }
                            }, toDisplayString(suggestion.title), 1),
                            createVNode(VList, { class: "card-list" }, {
                              default: withCtx(() => [
                                (openBlock(true), createBlock(Fragment, null, renderList(suggestion.content, (item) => {
                                  return openBlock(), createBlock(VListItem, {
                                    key: item.title,
                                    class: "app-bar-search-suggestion mx-4 mt-2",
                                    onClick: ($event) => redirectToSuggestedPage(item)
                                  }, {
                                    prepend: withCtx(() => [
                                      createVNode(VIcon, {
                                        icon: item.icon,
                                        size: "20",
                                        class: "me-n1"
                                      }, null, 8, ["icon"])
                                    ]),
                                    default: withCtx(() => [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode(toDisplayString(item.title), 1)
                                        ]),
                                        _: 2
                                      }, 1024)
                                    ]),
                                    _: 2
                                  }, 1032, ["onClick"]);
                                }), 128))
                              ]),
                              _: 2
                            }, 1024)
                          ]),
                          _: 2
                        }, 1024);
                      }), 64))
                    ]),
                    _: 1
                  })) : createCommentVNode("", true)
                ]),
                _: 1
              })
            ];
          }
        }),
        noDataSuggestion: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(`<div class="mt-9"${_scopeId}><span class="d-flex justify-center text-disabled mb-2"${_scopeId}>Try searching for</span><!--[-->`);
            ssrRenderList(noDataSuggestions, (suggestion) => {
              _push2(`<h6 class="app-bar-search-suggestion text-h6 font-weight-regular cursor-pointer py-2 px-4"${_scopeId}>`);
              _push2(ssrRenderComponent(VIcon, {
                size: "20",
                icon: suggestion.icon,
                class: "me-2"
              }, null, _parent2, _scopeId));
              _push2(`<span${_scopeId}>${ssrInterpolate(suggestion.title)}</span></h6>`);
            });
            _push2(`<!--]--></div>`);
          } else {
            return [
              createVNode("div", { class: "mt-9" }, [
                createVNode("span", { class: "d-flex justify-center text-disabled mb-2" }, "Try searching for"),
                (openBlock(), createBlock(Fragment, null, renderList(noDataSuggestions, (suggestion) => {
                  return createVNode("h6", {
                    key: suggestion.title,
                    class: "app-bar-search-suggestion text-h6 font-weight-regular cursor-pointer py-2 px-4",
                    onClick: ($event) => redirectToSuggestedPage(suggestion)
                  }, [
                    createVNode(VIcon, {
                      size: "20",
                      icon: suggestion.icon,
                      class: "me-2"
                    }, null, 8, ["icon"]),
                    createVNode("span", null, toDisplayString(suggestion.title), 1)
                  ], 8, ["onClick"]);
                }), 64))
              ])
            ];
          }
        }),
        searchResult: withCtx(({ item }, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VListSubheader, { class: "text-disabled custom-letter-spacing font-weight-regular ps-4" }, {
              default: withCtx((_, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(`${ssrInterpolate(item.title)}`);
                } else {
                  return [
                    createTextVNode(toDisplayString(item.title), 1)
                  ];
                }
              }),
              _: 2
            }, _parent2, _scopeId));
            _push2(`<!--[-->`);
            ssrRenderList(item.children, (list) => {
              _push2(ssrRenderComponent(VListItem, {
                key: list.title,
                to: list.url,
                onClick: closeSearchBar
              }, {
                prepend: withCtx((_, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(ssrRenderComponent(VIcon, {
                      size: "20",
                      icon: list.icon,
                      class: "me-n1"
                    }, null, _parent3, _scopeId2));
                  } else {
                    return [
                      createVNode(VIcon, {
                        size: "20",
                        icon: list.icon,
                        class: "me-n1"
                      }, null, 8, ["icon"])
                    ];
                  }
                }),
                append: withCtx((_, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(ssrRenderComponent(VIcon, {
                      size: "20",
                      icon: "tabler-corner-down-left",
                      class: "enter-icon flip-in-rtl"
                    }, null, _parent3, _scopeId2));
                  } else {
                    return [
                      createVNode(VIcon, {
                        size: "20",
                        icon: "tabler-corner-down-left",
                        class: "enter-icon flip-in-rtl"
                      })
                    ];
                  }
                }),
                default: withCtx((_, _push3, _parent3, _scopeId2) => {
                  if (_push3) {
                    _push3(ssrRenderComponent(VListItemTitle, null, {
                      default: withCtx((_2, _push4, _parent4, _scopeId3) => {
                        if (_push4) {
                          _push4(`${ssrInterpolate(list.title)}`);
                        } else {
                          return [
                            createTextVNode(toDisplayString(list.title), 1)
                          ];
                        }
                      }),
                      _: 2
                    }, _parent3, _scopeId2));
                  } else {
                    return [
                      createVNode(VListItemTitle, null, {
                        default: withCtx(() => [
                          createTextVNode(toDisplayString(list.title), 1)
                        ]),
                        _: 2
                      }, 1024)
                    ];
                  }
                }),
                _: 2
              }, _parent2, _scopeId));
            });
            _push2(`<!--]-->`);
          } else {
            return [
              createVNode(VListSubheader, { class: "text-disabled custom-letter-spacing font-weight-regular ps-4" }, {
                default: withCtx(() => [
                  createTextVNode(toDisplayString(item.title), 1)
                ]),
                _: 2
              }, 1024),
              (openBlock(true), createBlock(Fragment, null, renderList(item.children, (list) => {
                return openBlock(), createBlock(VListItem, {
                  key: list.title,
                  to: list.url,
                  onClick: closeSearchBar
                }, {
                  prepend: withCtx(() => [
                    createVNode(VIcon, {
                      size: "20",
                      icon: list.icon,
                      class: "me-n1"
                    }, null, 8, ["icon"])
                  ]),
                  append: withCtx(() => [
                    createVNode(VIcon, {
                      size: "20",
                      icon: "tabler-corner-down-left",
                      class: "enter-icon flip-in-rtl"
                    })
                  ]),
                  default: withCtx(() => [
                    createVNode(VListItemTitle, null, {
                      default: withCtx(() => [
                        createTextVNode(toDisplayString(list.title), 1)
                      ]),
                      _: 2
                    }, 1024)
                  ]),
                  _: 2
                }, 1032, ["to"]);
              }), 128))
            ];
          }
        }),
        _: 1
      }, _parent));
      _push(`<!--]-->`);
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/components/NavSearchBar.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
