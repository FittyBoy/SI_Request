import { ac as useMagicKeys, a as VDialog, b as VCard, d as VCardText, k as VTextField, V as VIcon, s as VDivider, W as VList, X as VListItem, ad as VSkeletonLoader, _ as _export_sfc } from "../server.mjs";
import { defineComponent, ref, watch, mergeProps, withCtx, unref, isRef, createVNode, withKeys, createTextVNode, toDisplayString, openBlock, createBlock, Fragment, renderList, renderSlot, withDirectives, vShow, createCommentVNode, useSSRContext } from "vue";
import { ssrRenderComponent, ssrRenderStyle, ssrRenderSlot, ssrRenderList, ssrInterpolate } from "vue/server-renderer";
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
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "AppBarSearch",
  __ssrInlineRender: true,
  props: {
    isDialogVisible: { type: Boolean },
    searchResults: {},
    isLoading: { type: Boolean }
  },
  emits: ["update:isDialogVisible", "search"],
  setup(__props, { emit: __emit }) {
    const props = __props;
    const emit = __emit;
    const { ctrl_k, meta_k } = useMagicKeys({
      passive: false,
      onEventFired(e) {
        if (e.ctrlKey && e.key === "k" && e.type === "keydown")
          e.preventDefault();
      }
    });
    const refSearchList = ref();
    const refSearchInput = ref();
    const searchQueryLocal = ref("");
    watch([
      ctrl_k,
      meta_k
    ], () => {
      emit("update:isDialogVisible", true);
    });
    const clearSearchAndCloseDialog = () => {
      searchQueryLocal.value = "";
      emit("update:isDialogVisible", false);
    };
    const getFocusOnSearchList = (e) => {
      var _a, _b;
      if (e.key === "ArrowDown") {
        e.preventDefault();
        (_a = refSearchList.value) == null ? void 0 : _a.focus("next");
      } else if (e.key === "ArrowUp") {
        e.preventDefault();
        (_b = refSearchList.value) == null ? void 0 : _b.focus("prev");
      }
    };
    const dialogModelValueUpdate = (val) => {
      searchQueryLocal.value = "";
      emit("update:isDialogVisible", val);
    };
    watch(
      () => props.isDialogVisible,
      () => {
        searchQueryLocal.value = "";
      }
    );
    return (_ctx, _push, _parent, _attrs) => {
      _push(ssrRenderComponent(VDialog, mergeProps({
        "max-width": "600",
        "model-value": props.isDialogVisible,
        height: _ctx.$vuetify.display.smAndUp ? "531" : "100%",
        fullscreen: _ctx.$vuetify.display.width < 600,
        class: "app-bar-search-dialog",
        "onUpdate:modelValue": dialogModelValueUpdate,
        onKeyup: clearSearchAndCloseDialog
      }, _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VCard, {
              height: "100%",
              width: "100%",
              class: "position-relative"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VCardText, {
                    class: "px-4",
                    style: { "padding-block": "1rem 1.2rem" }
                  }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VTextField, {
                          ref_key: "refSearchInput",
                          ref: refSearchInput,
                          modelValue: unref(searchQueryLocal),
                          "onUpdate:modelValue": [($event) => isRef(searchQueryLocal) ? searchQueryLocal.value = $event : null, ($event) => _ctx.$emit("search", unref(searchQueryLocal))],
                          autofocus: "",
                          density: "compact",
                          variant: "plain",
                          class: "app-bar-search-input",
                          onKeyup: clearSearchAndCloseDialog,
                          onKeydown: getFocusOnSearchList
                        }, {
                          "prepend-inner": withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(`<div class="d-flex align-center text-high-emphasis me-1" data-v-ae311f47${_scopeId4}>`);
                              _push5(ssrRenderComponent(VIcon, {
                                size: "24",
                                icon: "tabler-search"
                              }, null, _parent5, _scopeId4));
                              _push5(`</div>`);
                            } else {
                              return [
                                createVNode("div", { class: "d-flex align-center text-high-emphasis me-1" }, [
                                  createVNode(VIcon, {
                                    size: "24",
                                    icon: "tabler-search"
                                  })
                                ])
                              ];
                            }
                          }),
                          "append-inner": withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(`<div class="d-flex align-start" data-v-ae311f47${_scopeId4}><div class="text-base text-disabled cursor-pointer me-3" data-v-ae311f47${_scopeId4}> [esc] </div>`);
                              _push5(ssrRenderComponent(VIcon, {
                                icon: "tabler-x",
                                size: "24",
                                onClick: clearSearchAndCloseDialog
                              }, null, _parent5, _scopeId4));
                              _push5(`</div>`);
                            } else {
                              return [
                                createVNode("div", { class: "d-flex align-start" }, [
                                  createVNode("div", {
                                    class: "text-base text-disabled cursor-pointer me-3",
                                    onClick: clearSearchAndCloseDialog
                                  }, " [esc] "),
                                  createVNode(VIcon, {
                                    icon: "tabler-x",
                                    size: "24",
                                    onClick: clearSearchAndCloseDialog
                                  })
                                ])
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                      } else {
                        return [
                          createVNode(VTextField, {
                            ref_key: "refSearchInput",
                            ref: refSearchInput,
                            modelValue: unref(searchQueryLocal),
                            "onUpdate:modelValue": [($event) => isRef(searchQueryLocal) ? searchQueryLocal.value = $event : null, ($event) => _ctx.$emit("search", unref(searchQueryLocal))],
                            autofocus: "",
                            density: "compact",
                            variant: "plain",
                            class: "app-bar-search-input",
                            onKeyup: withKeys(clearSearchAndCloseDialog, ["esc"]),
                            onKeydown: getFocusOnSearchList
                          }, {
                            "prepend-inner": withCtx(() => [
                              createVNode("div", { class: "d-flex align-center text-high-emphasis me-1" }, [
                                createVNode(VIcon, {
                                  size: "24",
                                  icon: "tabler-search"
                                })
                              ])
                            ]),
                            "append-inner": withCtx(() => [
                              createVNode("div", { class: "d-flex align-start" }, [
                                createVNode("div", {
                                  class: "text-base text-disabled cursor-pointer me-3",
                                  onClick: clearSearchAndCloseDialog
                                }, " [esc] "),
                                createVNode(VIcon, {
                                  icon: "tabler-x",
                                  size: "24",
                                  onClick: clearSearchAndCloseDialog
                                })
                              ])
                            ]),
                            _: 1
                          }, 8, ["modelValue", "onUpdate:modelValue"])
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                  _push3(ssrRenderComponent(VDivider, null, null, _parent3, _scopeId2));
                  _push3(ssrRenderComponent(unref(PerfectScrollbar), {
                    options: { wheelPropagation: false, suppressScrollX: true },
                    class: "h-100"
                  }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(`<div style="${ssrRenderStyle(!!props.searchResults && !unref(searchQueryLocal) && _ctx.$slots.suggestions ? null : { display: "none" })}" class="h-100" data-v-ae311f47${_scopeId3}>`);
                        ssrRenderSlot(_ctx.$slots, "suggestions", {}, null, _push4, _parent4, _scopeId3);
                        _push4(`</div>`);
                        if (!_ctx.isLoading) {
                          _push4(`<!--[-->`);
                          _push4(ssrRenderComponent(unref(VList), {
                            style: unref(searchQueryLocal).length && !!props.searchResults.length ? null : { display: "none" },
                            ref_key: "refSearchList",
                            ref: refSearchList,
                            density: "compact",
                            class: "app-bar-search-list py-0"
                          }, {
                            default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                              if (_push5) {
                                _push5(`<!--[-->`);
                                ssrRenderList(props.searchResults, (item) => {
                                  ssrRenderSlot(_ctx.$slots, "searchResult", { item }, () => {
                                    _push5(ssrRenderComponent(unref(VListItem), null, {
                                      default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                        if (_push6) {
                                          _push6(`${ssrInterpolate(item)}`);
                                        } else {
                                          return [
                                            createTextVNode(toDisplayString(item), 1)
                                          ];
                                        }
                                      }),
                                      _: 2
                                    }, _parent5, _scopeId4));
                                  }, _push5, _parent5, _scopeId4);
                                });
                                _push5(`<!--]-->`);
                              } else {
                                return [
                                  (openBlock(true), createBlock(Fragment, null, renderList(props.searchResults, (item) => {
                                    return renderSlot(_ctx.$slots, "searchResult", {
                                      key: item,
                                      item
                                    }, () => [
                                      createVNode(unref(VListItem), null, {
                                        default: withCtx(() => [
                                          createTextVNode(toDisplayString(item), 1)
                                        ]),
                                        _: 2
                                      }, 1024)
                                    ], true);
                                  }), 128))
                                ];
                              }
                            }),
                            _: 3
                          }, _parent4, _scopeId3));
                          _push4(`<div style="${ssrRenderStyle(!props.searchResults.length && unref(searchQueryLocal).length ? null : { display: "none" })}" class="h-100" data-v-ae311f47${_scopeId3}>`);
                          ssrRenderSlot(_ctx.$slots, "noData", {}, () => {
                            _push4(ssrRenderComponent(VCardText, { class: "h-100" }, {
                              default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                                if (_push5) {
                                  _push5(`<div class="app-bar-search-suggestions d-flex flex-column align-center justify-center text-high-emphasis pa-12" data-v-ae311f47${_scopeId4}>`);
                                  _push5(ssrRenderComponent(VIcon, {
                                    size: "64",
                                    icon: "tabler-file-alert"
                                  }, null, _parent5, _scopeId4));
                                  _push5(`<div class="d-flex align-center flex-wrap justify-center gap-2 text-h5 mt-3" data-v-ae311f47${_scopeId4}><span data-v-ae311f47${_scopeId4}>No Result For </span><span data-v-ae311f47${_scopeId4}>&quot;${ssrInterpolate(unref(searchQueryLocal))}&quot;</span></div>`);
                                  ssrRenderSlot(_ctx.$slots, "noDataSuggestion", {}, null, _push5, _parent5, _scopeId4);
                                  _push5(`</div>`);
                                } else {
                                  return [
                                    createVNode("div", { class: "app-bar-search-suggestions d-flex flex-column align-center justify-center text-high-emphasis pa-12" }, [
                                      createVNode(VIcon, {
                                        size: "64",
                                        icon: "tabler-file-alert"
                                      }),
                                      createVNode("div", { class: "d-flex align-center flex-wrap justify-center gap-2 text-h5 mt-3" }, [
                                        createVNode("span", null, "No Result For "),
                                        createVNode("span", null, '"' + toDisplayString(unref(searchQueryLocal)) + '"', 1)
                                      ]),
                                      renderSlot(_ctx.$slots, "noDataSuggestion", {}, void 0, true)
                                    ])
                                  ];
                                }
                              }),
                              _: 3
                            }, _parent4, _scopeId3));
                          }, _push4, _parent4, _scopeId3);
                          _push4(`</div><!--]-->`);
                        } else {
                          _push4(`<!---->`);
                        }
                        if (_ctx.isLoading) {
                          _push4(`<!--[-->`);
                          ssrRenderList(3, (i) => {
                            _push4(ssrRenderComponent(VSkeletonLoader, {
                              key: i,
                              type: "list-item-two-line"
                            }, null, _parent4, _scopeId3));
                          });
                          _push4(`<!--]-->`);
                        } else {
                          _push4(`<!---->`);
                        }
                      } else {
                        return [
                          withDirectives(createVNode("div", { class: "h-100" }, [
                            renderSlot(_ctx.$slots, "suggestions", {}, void 0, true)
                          ], 512), [
                            [vShow, !!props.searchResults && !unref(searchQueryLocal) && _ctx.$slots.suggestions]
                          ]),
                          !_ctx.isLoading ? (openBlock(), createBlock(Fragment, { key: 0 }, [
                            withDirectives(createVNode(unref(VList), {
                              ref_key: "refSearchList",
                              ref: refSearchList,
                              density: "compact",
                              class: "app-bar-search-list py-0"
                            }, {
                              default: withCtx(() => [
                                (openBlock(true), createBlock(Fragment, null, renderList(props.searchResults, (item) => {
                                  return renderSlot(_ctx.$slots, "searchResult", {
                                    key: item,
                                    item
                                  }, () => [
                                    createVNode(unref(VListItem), null, {
                                      default: withCtx(() => [
                                        createTextVNode(toDisplayString(item), 1)
                                      ]),
                                      _: 2
                                    }, 1024)
                                  ], true);
                                }), 128))
                              ]),
                              _: 3
                            }, 512), [
                              [vShow, unref(searchQueryLocal).length && !!props.searchResults.length]
                            ]),
                            withDirectives(createVNode("div", { class: "h-100" }, [
                              renderSlot(_ctx.$slots, "noData", {}, () => [
                                createVNode(VCardText, { class: "h-100" }, {
                                  default: withCtx(() => [
                                    createVNode("div", { class: "app-bar-search-suggestions d-flex flex-column align-center justify-center text-high-emphasis pa-12" }, [
                                      createVNode(VIcon, {
                                        size: "64",
                                        icon: "tabler-file-alert"
                                      }),
                                      createVNode("div", { class: "d-flex align-center flex-wrap justify-center gap-2 text-h5 mt-3" }, [
                                        createVNode("span", null, "No Result For "),
                                        createVNode("span", null, '"' + toDisplayString(unref(searchQueryLocal)) + '"', 1)
                                      ]),
                                      renderSlot(_ctx.$slots, "noDataSuggestion", {}, void 0, true)
                                    ])
                                  ]),
                                  _: 3
                                })
                              ], true)
                            ], 512), [
                              [vShow, !props.searchResults.length && unref(searchQueryLocal).length]
                            ])
                          ], 64)) : createCommentVNode("", true),
                          _ctx.isLoading ? (openBlock(), createBlock(Fragment, { key: 1 }, renderList(3, (i) => {
                            return createVNode(VSkeletonLoader, {
                              key: i,
                              type: "list-item-two-line"
                            });
                          }), 64)) : createCommentVNode("", true)
                        ];
                      }
                    }),
                    _: 3
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VCardText, {
                      class: "px-4",
                      style: { "padding-block": "1rem 1.2rem" }
                    }, {
                      default: withCtx(() => [
                        createVNode(VTextField, {
                          ref_key: "refSearchInput",
                          ref: refSearchInput,
                          modelValue: unref(searchQueryLocal),
                          "onUpdate:modelValue": [($event) => isRef(searchQueryLocal) ? searchQueryLocal.value = $event : null, ($event) => _ctx.$emit("search", unref(searchQueryLocal))],
                          autofocus: "",
                          density: "compact",
                          variant: "plain",
                          class: "app-bar-search-input",
                          onKeyup: withKeys(clearSearchAndCloseDialog, ["esc"]),
                          onKeydown: getFocusOnSearchList
                        }, {
                          "prepend-inner": withCtx(() => [
                            createVNode("div", { class: "d-flex align-center text-high-emphasis me-1" }, [
                              createVNode(VIcon, {
                                size: "24",
                                icon: "tabler-search"
                              })
                            ])
                          ]),
                          "append-inner": withCtx(() => [
                            createVNode("div", { class: "d-flex align-start" }, [
                              createVNode("div", {
                                class: "text-base text-disabled cursor-pointer me-3",
                                onClick: clearSearchAndCloseDialog
                              }, " [esc] "),
                              createVNode(VIcon, {
                                icon: "tabler-x",
                                size: "24",
                                onClick: clearSearchAndCloseDialog
                              })
                            ])
                          ]),
                          _: 1
                        }, 8, ["modelValue", "onUpdate:modelValue"])
                      ]),
                      _: 1
                    }),
                    createVNode(VDivider),
                    createVNode(unref(PerfectScrollbar), {
                      options: { wheelPropagation: false, suppressScrollX: true },
                      class: "h-100"
                    }, {
                      default: withCtx(() => [
                        withDirectives(createVNode("div", { class: "h-100" }, [
                          renderSlot(_ctx.$slots, "suggestions", {}, void 0, true)
                        ], 512), [
                          [vShow, !!props.searchResults && !unref(searchQueryLocal) && _ctx.$slots.suggestions]
                        ]),
                        !_ctx.isLoading ? (openBlock(), createBlock(Fragment, { key: 0 }, [
                          withDirectives(createVNode(unref(VList), {
                            ref_key: "refSearchList",
                            ref: refSearchList,
                            density: "compact",
                            class: "app-bar-search-list py-0"
                          }, {
                            default: withCtx(() => [
                              (openBlock(true), createBlock(Fragment, null, renderList(props.searchResults, (item) => {
                                return renderSlot(_ctx.$slots, "searchResult", {
                                  key: item,
                                  item
                                }, () => [
                                  createVNode(unref(VListItem), null, {
                                    default: withCtx(() => [
                                      createTextVNode(toDisplayString(item), 1)
                                    ]),
                                    _: 2
                                  }, 1024)
                                ], true);
                              }), 128))
                            ]),
                            _: 3
                          }, 512), [
                            [vShow, unref(searchQueryLocal).length && !!props.searchResults.length]
                          ]),
                          withDirectives(createVNode("div", { class: "h-100" }, [
                            renderSlot(_ctx.$slots, "noData", {}, () => [
                              createVNode(VCardText, { class: "h-100" }, {
                                default: withCtx(() => [
                                  createVNode("div", { class: "app-bar-search-suggestions d-flex flex-column align-center justify-center text-high-emphasis pa-12" }, [
                                    createVNode(VIcon, {
                                      size: "64",
                                      icon: "tabler-file-alert"
                                    }),
                                    createVNode("div", { class: "d-flex align-center flex-wrap justify-center gap-2 text-h5 mt-3" }, [
                                      createVNode("span", null, "No Result For "),
                                      createVNode("span", null, '"' + toDisplayString(unref(searchQueryLocal)) + '"', 1)
                                    ]),
                                    renderSlot(_ctx.$slots, "noDataSuggestion", {}, void 0, true)
                                  ])
                                ]),
                                _: 3
                              })
                            ], true)
                          ], 512), [
                            [vShow, !props.searchResults.length && unref(searchQueryLocal).length]
                          ])
                        ], 64)) : createCommentVNode("", true),
                        _ctx.isLoading ? (openBlock(), createBlock(Fragment, { key: 1 }, renderList(3, (i) => {
                          return createVNode(VSkeletonLoader, {
                            key: i,
                            type: "list-item-two-line"
                          });
                        }), 64)) : createCommentVNode("", true)
                      ]),
                      _: 3
                    })
                  ];
                }
              }),
              _: 3
            }, _parent2, _scopeId));
          } else {
            return [
              createVNode(VCard, {
                height: "100%",
                width: "100%",
                class: "position-relative"
              }, {
                default: withCtx(() => [
                  createVNode(VCardText, {
                    class: "px-4",
                    style: { "padding-block": "1rem 1.2rem" }
                  }, {
                    default: withCtx(() => [
                      createVNode(VTextField, {
                        ref_key: "refSearchInput",
                        ref: refSearchInput,
                        modelValue: unref(searchQueryLocal),
                        "onUpdate:modelValue": [($event) => isRef(searchQueryLocal) ? searchQueryLocal.value = $event : null, ($event) => _ctx.$emit("search", unref(searchQueryLocal))],
                        autofocus: "",
                        density: "compact",
                        variant: "plain",
                        class: "app-bar-search-input",
                        onKeyup: withKeys(clearSearchAndCloseDialog, ["esc"]),
                        onKeydown: getFocusOnSearchList
                      }, {
                        "prepend-inner": withCtx(() => [
                          createVNode("div", { class: "d-flex align-center text-high-emphasis me-1" }, [
                            createVNode(VIcon, {
                              size: "24",
                              icon: "tabler-search"
                            })
                          ])
                        ]),
                        "append-inner": withCtx(() => [
                          createVNode("div", { class: "d-flex align-start" }, [
                            createVNode("div", {
                              class: "text-base text-disabled cursor-pointer me-3",
                              onClick: clearSearchAndCloseDialog
                            }, " [esc] "),
                            createVNode(VIcon, {
                              icon: "tabler-x",
                              size: "24",
                              onClick: clearSearchAndCloseDialog
                            })
                          ])
                        ]),
                        _: 1
                      }, 8, ["modelValue", "onUpdate:modelValue"])
                    ]),
                    _: 1
                  }),
                  createVNode(VDivider),
                  createVNode(unref(PerfectScrollbar), {
                    options: { wheelPropagation: false, suppressScrollX: true },
                    class: "h-100"
                  }, {
                    default: withCtx(() => [
                      withDirectives(createVNode("div", { class: "h-100" }, [
                        renderSlot(_ctx.$slots, "suggestions", {}, void 0, true)
                      ], 512), [
                        [vShow, !!props.searchResults && !unref(searchQueryLocal) && _ctx.$slots.suggestions]
                      ]),
                      !_ctx.isLoading ? (openBlock(), createBlock(Fragment, { key: 0 }, [
                        withDirectives(createVNode(unref(VList), {
                          ref_key: "refSearchList",
                          ref: refSearchList,
                          density: "compact",
                          class: "app-bar-search-list py-0"
                        }, {
                          default: withCtx(() => [
                            (openBlock(true), createBlock(Fragment, null, renderList(props.searchResults, (item) => {
                              return renderSlot(_ctx.$slots, "searchResult", {
                                key: item,
                                item
                              }, () => [
                                createVNode(unref(VListItem), null, {
                                  default: withCtx(() => [
                                    createTextVNode(toDisplayString(item), 1)
                                  ]),
                                  _: 2
                                }, 1024)
                              ], true);
                            }), 128))
                          ]),
                          _: 3
                        }, 512), [
                          [vShow, unref(searchQueryLocal).length && !!props.searchResults.length]
                        ]),
                        withDirectives(createVNode("div", { class: "h-100" }, [
                          renderSlot(_ctx.$slots, "noData", {}, () => [
                            createVNode(VCardText, { class: "h-100" }, {
                              default: withCtx(() => [
                                createVNode("div", { class: "app-bar-search-suggestions d-flex flex-column align-center justify-center text-high-emphasis pa-12" }, [
                                  createVNode(VIcon, {
                                    size: "64",
                                    icon: "tabler-file-alert"
                                  }),
                                  createVNode("div", { class: "d-flex align-center flex-wrap justify-center gap-2 text-h5 mt-3" }, [
                                    createVNode("span", null, "No Result For "),
                                    createVNode("span", null, '"' + toDisplayString(unref(searchQueryLocal)) + '"', 1)
                                  ]),
                                  renderSlot(_ctx.$slots, "noDataSuggestion", {}, void 0, true)
                                ])
                              ]),
                              _: 3
                            })
                          ], true)
                        ], 512), [
                          [vShow, !props.searchResults.length && unref(searchQueryLocal).length]
                        ])
                      ], 64)) : createCommentVNode("", true),
                      _ctx.isLoading ? (openBlock(), createBlock(Fragment, { key: 1 }, renderList(3, (i) => {
                        return createVNode(VSkeletonLoader, {
                          key: i,
                          type: "list-item-two-line"
                        });
                      }), 64)) : createCommentVNode("", true)
                    ]),
                    _: 3
                  })
                ]),
                _: 3
              })
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
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@core/components/AppBarSearch.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
const AppBarSearch = /* @__PURE__ */ _export_sfc(_sfc_main, [["__scopeId", "data-v-ae311f47"]]);
export {
  AppBarSearch as default
};
