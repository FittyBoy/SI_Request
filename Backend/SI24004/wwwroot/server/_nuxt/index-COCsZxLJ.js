import { defineComponent, resolveComponent, mergeProps, withCtx, createVNode, useSSRContext, ref, toRaw, watch, unref, createTextVNode, toDisplayString, withModifiers, withAsyncContext, computed } from "vue";
import { ssrRenderComponent, ssrInterpolate } from "vue/server-renderer";
import { V as VIcon, a as VDialog, b as VCard, d as VCardText, e as VForm, f as VRow, g as VCol, h as VBtn, u as useRuntimeConfig, i as useCookie, j as VCardTitle, k as VTextField, l as VDataTable, m as VChip } from "../server.mjs";
import { _ as _sfc_main$3 } from "./AppTextField-CcWZXBeD.js";
import "hookable";
import { u as useFetch, a as useApi } from "./useApi-DkwIRz4i.js";
import "#internal/nitro";
import "ofetch";
import "unctx";
import "h3";
import "ufo";
import "defu";
import "devalue";
import "cookie-es";
import "@antfu/utils";
import "axios";
const _sfc_main$2 = /* @__PURE__ */ defineComponent({
  __name: "DialogCloseBtn",
  __ssrInlineRender: true,
  props: {
    icon: { default: "tabler-x" },
    iconSize: { default: "20" }
  },
  setup(__props) {
    const props = __props;
    return (_ctx, _push, _parent, _attrs) => {
      const _component_IconBtn = resolveComponent("IconBtn");
      _push(ssrRenderComponent(_component_IconBtn, mergeProps({
        variant: "elevated",
        size: "30",
        ripple: false,
        class: "v-dialog-close-btn"
      }, _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VIcon, {
              icon: props.icon,
              size: props.iconSize
            }, null, _parent2, _scopeId));
          } else {
            return [
              createVNode(VIcon, {
                icon: props.icon,
                size: props.iconSize
              }, null, 8, ["icon", "size"])
            ];
          }
        }),
        _: 1
      }, _parent));
    };
  }
});
const _sfc_setup$2 = _sfc_main$2.setup;
_sfc_main$2.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@core/components/DialogCloseBtn.vue");
  return _sfc_setup$2 ? _sfc_setup$2(props, ctx) : void 0;
};
const _sfc_main$1 = /* @__PURE__ */ defineComponent({
  __name: "AddEditRequestDialog",
  __ssrInlineRender: true,
  props: {
    requestDTO: { default: () => ({
      id: "",
      requestName: "",
      userId: "",
      requestDescription: "",
      requestDate: null,
      requestApprove: false,
      approveDate: null,
      active: true,
      isDeleted: false,
      attachement: [
        {
          id: "",
          AttachmentName: "",
          AttachementPath: "",
          AttachementType: null,
          UploadDate: null,
          requestApprove: false,
          AttachementFileData: "",
          isDeleted: false
        }
      ]
    }) },
    isDialogVisible: { type: Boolean }
  },
  emits: ["submit", "update:isDialogVisible"],
  setup(__props, { emit: __emit }) {
    const refVFormRequest = ref();
    const isSubmitting = ref(false);
    const requiredValidator = (value) => !!value || "This field is required.";
    const props = __props;
    const emit = __emit;
    ref([]);
    const formInput = ref(structuredClone(toRaw(props.requestDTO)));
    watch(props, () => {
      formInput.value = structuredClone(toRaw(props.requestDTO));
    });
    const onFormSubmit = async () => {
      var _a;
      const validationResult = await ((_a = refVFormRequest.value) == null ? void 0 : _a.validate());
      if (!validationResult || !validationResult.valid) {
        return;
      }
      isSubmitting.value = true;
      try {
        await emit("submit", formInput.value);
        emit("update:isDialogVisible", false);
      } catch (error) {
        console.error("Failed to submit form:", error);
      } finally {
        isSubmitting.value = false;
      }
    };
    const onFormReset = () => {
      var _a;
      (_a = refVFormRequest.value) == null ? void 0 : _a.resetValidation();
      formInput.value = structuredClone(toRaw(props.requestDTO));
      emit("update:isDialogVisible", false);
    };
    return (_ctx, _push, _parent, _attrs) => {
      const _component_DialogCloseBtn = _sfc_main$2;
      const _component_AppTextField = _sfc_main$3;
      _push(ssrRenderComponent(VDialog, mergeProps({
        width: _ctx.$vuetify.display.smAndDown ? "100%" : "900px",
        "model-value": props.isDialogVisible,
        "onUpdate:modelValue": onFormReset
      }, _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(_component_DialogCloseBtn, { onClick: onFormReset }, null, _parent2, _scopeId));
            _push2(ssrRenderComponent(VCard, { class: "pa-sm-10 pa-2" }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VCardText, null, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(`<h4 class="text-h4 text-center mb-2"${_scopeId3}>${ssrInterpolate(props.requestDTO.id ? "Edit" : "Add")} Request </h4>`);
                        _push4(ssrRenderComponent(unref(VForm), {
                          class: "mt-6",
                          ref_key: "refVFormRequest",
                          ref: refVFormRequest,
                          onSubmit: onFormSubmit
                        }, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VRow, null, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VCol, { cols: "6" }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(ssrRenderComponent(_component_AppTextField, {
                                            modelValue: unref(formInput).requestName,
                                            "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                            rules: [requiredValidator],
                                            label: "requestName",
                                            type: "text",
                                            placeholder: "กรุณากรอกชื่อ"
                                          }, null, _parent7, _scopeId6));
                                        } else {
                                          return [
                                            createVNode(_component_AppTextField, {
                                              modelValue: unref(formInput).requestName,
                                              "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                              rules: [requiredValidator],
                                              label: "requestName",
                                              type: "text",
                                              placeholder: "กรุณากรอกชื่อ"
                                            }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                    _push6(ssrRenderComponent(VCol, { cols: "6" }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(ssrRenderComponent(_component_AppTextField, {
                                            modelValue: unref(formInput).requestDescription,
                                            "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                            rules: [requiredValidator],
                                            label: "Request Description",
                                            type: "text",
                                            placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                          }, null, _parent7, _scopeId6));
                                        } else {
                                          return [
                                            createVNode(_component_AppTextField, {
                                              modelValue: unref(formInput).requestDescription,
                                              "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                              rules: [requiredValidator],
                                              label: "Request Description",
                                              type: "text",
                                              placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                            }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                    _push6(ssrRenderComponent(VCol, {
                                      cols: "12",
                                      class: "d-flex flex-wrap justify-center gap-4"
                                    }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(ssrRenderComponent(VBtn, {
                                            loading: unref(isSubmitting),
                                            type: "submit"
                                          }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(` Submit `);
                                              } else {
                                                return [
                                                  createTextVNode(" Submit ")
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                          _push7(ssrRenderComponent(VBtn, {
                                            color: "secondary",
                                            variant: "tonal",
                                            onClick: onFormReset
                                          }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(` Cancel `);
                                              } else {
                                                return [
                                                  createTextVNode(" Cancel ")
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                        } else {
                                          return [
                                            createVNode(VBtn, {
                                              loading: unref(isSubmitting),
                                              type: "submit"
                                            }, {
                                              default: withCtx(() => [
                                                createTextVNode(" Submit ")
                                              ]),
                                              _: 1
                                            }, 8, ["loading"]),
                                            createVNode(VBtn, {
                                              color: "secondary",
                                              variant: "tonal",
                                              onClick: onFormReset
                                            }, {
                                              default: withCtx(() => [
                                                createTextVNode(" Cancel ")
                                              ]),
                                              _: 1
                                            })
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VCol, { cols: "6" }, {
                                        default: withCtx(() => [
                                          createVNode(_component_AppTextField, {
                                            modelValue: unref(formInput).requestName,
                                            "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                            rules: [requiredValidator],
                                            label: "requestName",
                                            type: "text",
                                            placeholder: "กรุณากรอกชื่อ"
                                          }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VCol, { cols: "6" }, {
                                        default: withCtx(() => [
                                          createVNode(_component_AppTextField, {
                                            modelValue: unref(formInput).requestDescription,
                                            "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                            rules: [requiredValidator],
                                            label: "Request Description",
                                            type: "text",
                                            placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                          }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VCol, {
                                        cols: "12",
                                        class: "d-flex flex-wrap justify-center gap-4"
                                      }, {
                                        default: withCtx(() => [
                                          createVNode(VBtn, {
                                            loading: unref(isSubmitting),
                                            type: "submit"
                                          }, {
                                            default: withCtx(() => [
                                              createTextVNode(" Submit ")
                                            ]),
                                            _: 1
                                          }, 8, ["loading"]),
                                          createVNode(VBtn, {
                                            color: "secondary",
                                            variant: "tonal",
                                            onClick: onFormReset
                                          }, {
                                            default: withCtx(() => [
                                              createTextVNode(" Cancel ")
                                            ]),
                                            _: 1
                                          })
                                        ]),
                                        _: 1
                                      })
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                            } else {
                              return [
                                createVNode(VRow, null, {
                                  default: withCtx(() => [
                                    createVNode(VCol, { cols: "6" }, {
                                      default: withCtx(() => [
                                        createVNode(_component_AppTextField, {
                                          modelValue: unref(formInput).requestName,
                                          "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                          rules: [requiredValidator],
                                          label: "requestName",
                                          type: "text",
                                          placeholder: "กรุณากรอกชื่อ"
                                        }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VCol, { cols: "6" }, {
                                      default: withCtx(() => [
                                        createVNode(_component_AppTextField, {
                                          modelValue: unref(formInput).requestDescription,
                                          "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                          rules: [requiredValidator],
                                          label: "Request Description",
                                          type: "text",
                                          placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                        }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VCol, {
                                      cols: "12",
                                      class: "d-flex flex-wrap justify-center gap-4"
                                    }, {
                                      default: withCtx(() => [
                                        createVNode(VBtn, {
                                          loading: unref(isSubmitting),
                                          type: "submit"
                                        }, {
                                          default: withCtx(() => [
                                            createTextVNode(" Submit ")
                                          ]),
                                          _: 1
                                        }, 8, ["loading"]),
                                        createVNode(VBtn, {
                                          color: "secondary",
                                          variant: "tonal",
                                          onClick: onFormReset
                                        }, {
                                          default: withCtx(() => [
                                            createTextVNode(" Cancel ")
                                          ]),
                                          _: 1
                                        })
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                })
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                      } else {
                        return [
                          createVNode("h4", { class: "text-h4 text-center mb-2" }, toDisplayString(props.requestDTO.id ? "Edit" : "Add") + " Request ", 1),
                          createVNode(unref(VForm), {
                            class: "mt-6",
                            ref_key: "refVFormRequest",
                            ref: refVFormRequest,
                            onSubmit: withModifiers(onFormSubmit, ["prevent"])
                          }, {
                            default: withCtx(() => [
                              createVNode(VRow, null, {
                                default: withCtx(() => [
                                  createVNode(VCol, { cols: "6" }, {
                                    default: withCtx(() => [
                                      createVNode(_component_AppTextField, {
                                        modelValue: unref(formInput).requestName,
                                        "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                        rules: [requiredValidator],
                                        label: "requestName",
                                        type: "text",
                                        placeholder: "กรุณากรอกชื่อ"
                                      }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VCol, { cols: "6" }, {
                                    default: withCtx(() => [
                                      createVNode(_component_AppTextField, {
                                        modelValue: unref(formInput).requestDescription,
                                        "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                        rules: [requiredValidator],
                                        label: "Request Description",
                                        type: "text",
                                        placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                      }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VCol, {
                                    cols: "12",
                                    class: "d-flex flex-wrap justify-center gap-4"
                                  }, {
                                    default: withCtx(() => [
                                      createVNode(VBtn, {
                                        loading: unref(isSubmitting),
                                        type: "submit"
                                      }, {
                                        default: withCtx(() => [
                                          createTextVNode(" Submit ")
                                        ]),
                                        _: 1
                                      }, 8, ["loading"]),
                                      createVNode(VBtn, {
                                        color: "secondary",
                                        variant: "tonal",
                                        onClick: onFormReset
                                      }, {
                                        default: withCtx(() => [
                                          createTextVNode(" Cancel ")
                                        ]),
                                        _: 1
                                      })
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }, 512)
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VCardText, null, {
                      default: withCtx(() => [
                        createVNode("h4", { class: "text-h4 text-center mb-2" }, toDisplayString(props.requestDTO.id ? "Edit" : "Add") + " Request ", 1),
                        createVNode(unref(VForm), {
                          class: "mt-6",
                          ref_key: "refVFormRequest",
                          ref: refVFormRequest,
                          onSubmit: withModifiers(onFormSubmit, ["prevent"])
                        }, {
                          default: withCtx(() => [
                            createVNode(VRow, null, {
                              default: withCtx(() => [
                                createVNode(VCol, { cols: "6" }, {
                                  default: withCtx(() => [
                                    createVNode(_component_AppTextField, {
                                      modelValue: unref(formInput).requestName,
                                      "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                      rules: [requiredValidator],
                                      label: "requestName",
                                      type: "text",
                                      placeholder: "กรุณากรอกชื่อ"
                                    }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                  ]),
                                  _: 1
                                }),
                                createVNode(VCol, { cols: "6" }, {
                                  default: withCtx(() => [
                                    createVNode(_component_AppTextField, {
                                      modelValue: unref(formInput).requestDescription,
                                      "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                      rules: [requiredValidator],
                                      label: "Request Description",
                                      type: "text",
                                      placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                    }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                  ]),
                                  _: 1
                                }),
                                createVNode(VCol, {
                                  cols: "12",
                                  class: "d-flex flex-wrap justify-center gap-4"
                                }, {
                                  default: withCtx(() => [
                                    createVNode(VBtn, {
                                      loading: unref(isSubmitting),
                                      type: "submit"
                                    }, {
                                      default: withCtx(() => [
                                        createTextVNode(" Submit ")
                                      ]),
                                      _: 1
                                    }, 8, ["loading"]),
                                    createVNode(VBtn, {
                                      color: "secondary",
                                      variant: "tonal",
                                      onClick: onFormReset
                                    }, {
                                      default: withCtx(() => [
                                        createTextVNode(" Cancel ")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            })
                          ]),
                          _: 1
                        }, 512)
                      ]),
                      _: 1
                    })
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
          } else {
            return [
              createVNode(_component_DialogCloseBtn, { onClick: onFormReset }),
              createVNode(VCard, { class: "pa-sm-10 pa-2" }, {
                default: withCtx(() => [
                  createVNode(VCardText, null, {
                    default: withCtx(() => [
                      createVNode("h4", { class: "text-h4 text-center mb-2" }, toDisplayString(props.requestDTO.id ? "Edit" : "Add") + " Request ", 1),
                      createVNode(unref(VForm), {
                        class: "mt-6",
                        ref_key: "refVFormRequest",
                        ref: refVFormRequest,
                        onSubmit: withModifiers(onFormSubmit, ["prevent"])
                      }, {
                        default: withCtx(() => [
                          createVNode(VRow, null, {
                            default: withCtx(() => [
                              createVNode(VCol, { cols: "6" }, {
                                default: withCtx(() => [
                                  createVNode(_component_AppTextField, {
                                    modelValue: unref(formInput).requestName,
                                    "onUpdate:modelValue": ($event) => unref(formInput).requestName = $event,
                                    rules: [requiredValidator],
                                    label: "requestName",
                                    type: "text",
                                    placeholder: "กรุณากรอกชื่อ"
                                  }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                ]),
                                _: 1
                              }),
                              createVNode(VCol, { cols: "6" }, {
                                default: withCtx(() => [
                                  createVNode(_component_AppTextField, {
                                    modelValue: unref(formInput).requestDescription,
                                    "onUpdate:modelValue": ($event) => unref(formInput).requestDescription = $event,
                                    rules: [requiredValidator],
                                    label: "Request Description",
                                    type: "text",
                                    placeholder: "กรุณากรอกรายละเอียดของใบนำเสนอ"
                                  }, null, 8, ["modelValue", "onUpdate:modelValue", "rules"])
                                ]),
                                _: 1
                              }),
                              createVNode(VCol, {
                                cols: "12",
                                class: "d-flex flex-wrap justify-center gap-4"
                              }, {
                                default: withCtx(() => [
                                  createVNode(VBtn, {
                                    loading: unref(isSubmitting),
                                    type: "submit"
                                  }, {
                                    default: withCtx(() => [
                                      createTextVNode(" Submit ")
                                    ]),
                                    _: 1
                                  }, 8, ["loading"]),
                                  createVNode(VBtn, {
                                    color: "secondary",
                                    variant: "tonal",
                                    onClick: onFormReset
                                  }, {
                                    default: withCtx(() => [
                                      createTextVNode(" Cancel ")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          })
                        ]),
                        _: 1
                      }, 512)
                    ]),
                    _: 1
                  })
                ]),
                _: 1
              })
            ];
          }
        }),
        _: 1
      }, _parent));
    };
  }
});
const _sfc_setup$1 = _sfc_main$1.setup;
_sfc_main$1.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("components/dialogs/AddEditRequestDialog.vue");
  return _sfc_setup$1 ? _sfc_setup$1(props, ctx) : void 0;
};
const $api = $fetch.create({
  // Request interceptor
  async onRequest({ options }) {
    options.baseURL = useRuntimeConfig().public.apiBaseUrl || "/api";
    const accessToken = useCookie("accessToken").value;
    if (accessToken) {
      options.headers = {
        ...options.headers,
        Authorization: `Bearer ${accessToken}`
      };
    }
  }
});
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "index",
  __ssrInlineRender: true,
  async setup(__props) {
    let __temp, __restore;
    const isloading = ref(true);
    ref(false);
    const itemsPerPage = ref(10);
    const page = ref(1);
    ref(false);
    ref();
    ref();
    const search = ref("");
    const headers = [
      { title: "::", key: "no" },
      { title: "Request Code", key: "requestCode" },
      { title: "Request Name", key: "requestName" },
      { title: "Request Description", key: "requestDescription" },
      { title: "Request Approve", key: "requestApprove" },
      { title: "active", key: "active", sortable: false },
      { text: "Actions", value: "actions", sortable: false }
    ];
    const isRequestDialogVisible = ref(false);
    const { data: fetchedData, error } = ([__temp, __restore] = withAsyncContext(() => useFetch("https://localhost:7247/api/SI24004AVI", {
      method: "GET",
      headers: {
        Authorization: "Bearer <your_token>",
        "Content-Type": "application/json"
      }
    }, "$pqtWcjQkdb")), __temp = await __temp, __restore(), __temp);
    if (error.value) {
      console.error("Failed to fetch data:", error.value);
      isloading.value = false;
    }
    const requestInfo = useCookie("requestData");
    const { data: requestData, execute: fetchRequest } = ([__temp, __restore] = withAsyncContext(() => {
      var _a;
      return useApi(
        ((_a = requestInfo.value) == null ? void 0 : _a.id) ? `/api/SI24004AVI?id=${requestInfo.value.id}` : `/api/SI24004AVI`,
        // กรณีไม่มี id
        { method: "GET" }
      );
    }), __temp = await __temp, __restore(), __temp);
    const itemDetail = computed(() => requestData.value || []);
    computed(
      () => itemDetail.value.filter((item) => {
        return item.requestName.toLowerCase().includes(search.value.toLowerCase()) || item.requestName.toLowerCase().includes(search.value.toLowerCase());
      })
    );
    const selectedItem = ref(null);
    const handleEdit = (item) => {
      selectedItem.value = item;
      isRequestDialogVisible.value = true;
    };
    const openAddDialog = () => {
      selectedItem.value = null;
      isRequestDialogVisible.value = true;
    };
    const handleDelete = async (id) => {
      try {
        const confirmed = confirm("Are you sure you want to delete this item?");
        if (confirmed) {
          await $api(`/api/SI24004AVI?id=${id}`, { method: "DELETE" });
          await fetchRequest();
        }
      } catch (error2) {
        console.error("Error deleting data:", error2);
      }
    };
    const onSubmit = async (item) => {
      try {
        if (item == null ? void 0 : item.id) {
          await $api(`/api/SI24004AVI?id=${item.id}`, {
            method: "PUT",
            // ใช้ PUT สำหรับการแก้ไข
            body: item
          });
        } else {
          await $api(`/api/SI24004AVI`, {
            method: "POST",
            body: item
          });
        }
        await fetchRequest();
      } catch (error2) {
        console.error("Error submitting data:", error2);
      }
    };
    ref(fetchedData.value || []);
    return (_ctx, _push, _parent, _attrs) => {
      const _component_IconBtn = resolveComponent("IconBtn");
      const _component_AddEditRequestDialog = _sfc_main$1;
      _push(`<!--[-->`);
      _push(ssrRenderComponent(VCard, null, {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VCardTitle, null, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(`Data Table`);
                } else {
                  return [
                    createTextVNode("Data Table")
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
            _push2(ssrRenderComponent(VRow, {
              class: "d-flex align-center",
              justify: "space-between"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VCol, { cols: "11" }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VTextField, {
                          modelValue: search.value,
                          "onUpdate:modelValue": ($event) => search.value = $event,
                          label: "Search",
                          class: "mb-4",
                          placeholder: "request code/request name"
                        }, null, _parent4, _scopeId3));
                      } else {
                        return [
                          createVNode(VTextField, {
                            modelValue: search.value,
                            "onUpdate:modelValue": ($event) => search.value = $event,
                            label: "Search",
                            class: "mb-4",
                            placeholder: "request code/request name"
                          }, null, 8, ["modelValue", "onUpdate:modelValue"])
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                  _push3(ssrRenderComponent(VCol, { cols: "1" }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VBtn, {
                          color: "primary",
                          onClick: openAddDialog
                        }, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(`Add New`);
                            } else {
                              return [
                                createTextVNode("Add New")
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                      } else {
                        return [
                          createVNode(VBtn, {
                            color: "primary",
                            onClick: openAddDialog
                          }, {
                            default: withCtx(() => [
                              createTextVNode("Add New")
                            ]),
                            _: 1
                          })
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VCol, { cols: "11" }, {
                      default: withCtx(() => [
                        createVNode(VTextField, {
                          modelValue: search.value,
                          "onUpdate:modelValue": ($event) => search.value = $event,
                          label: "Search",
                          class: "mb-4",
                          placeholder: "request code/request name"
                        }, null, 8, ["modelValue", "onUpdate:modelValue"])
                      ]),
                      _: 1
                    }),
                    createVNode(VCol, { cols: "1" }, {
                      default: withCtx(() => [
                        createVNode(VBtn, {
                          color: "primary",
                          onClick: openAddDialog
                        }, {
                          default: withCtx(() => [
                            createTextVNode("Add New")
                          ]),
                          _: 1
                        })
                      ]),
                      _: 1
                    })
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
            _push2(ssrRenderComponent(VDataTable, {
              headers,
              items: unref(itemDetail),
              "items-per-page": itemsPerPage.value,
              page: page.value,
              class: "elevation-1"
            }, {
              "item.no": withCtx(({ index }, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(`<div class="text-body-1 text-high-emphasis text-capitalize"${_scopeId2}>${ssrInterpolate((page.value - 1) * itemsPerPage.value + 1 + index)}</div>`);
                } else {
                  return [
                    createVNode("div", { class: "text-body-1 text-high-emphasis text-capitalize" }, toDisplayString((page.value - 1) * itemsPerPage.value + 1 + index), 1)
                  ];
                }
              }),
              "item.requestApprove": withCtx(({ item }, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VChip, {
                    style: {
                      backgroundColor: item.requestApprove ? "#d4edda" : "#f8d7da",
                      color: item.requestApprove ? "#155724" : "#721c24",
                      padding: "8px",
                      textAlign: "center"
                    }
                  }, {
                    default: withCtx((_2, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(`${ssrInterpolate(item.requestApprove ? "Pass" : "Not Pass")}`);
                      } else {
                        return [
                          createTextVNode(toDisplayString(item.requestApprove ? "Pass" : "Not Pass"), 1)
                        ];
                      }
                    }),
                    _: 2
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VChip, {
                      style: {
                        backgroundColor: item.requestApprove ? "#d4edda" : "#f8d7da",
                        color: item.requestApprove ? "#155724" : "#721c24",
                        padding: "8px",
                        textAlign: "center"
                      }
                    }, {
                      default: withCtx(() => [
                        createTextVNode(toDisplayString(item.requestApprove ? "Pass" : "Not Pass"), 1)
                      ]),
                      _: 2
                    }, 1032, ["style"])
                  ];
                }
              }),
              "item.active": withCtx(({ item }, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VChip, {
                    style: {
                      backgroundColor: item.active ? "#d4edda" : "#f8d7da",
                      color: item.active ? "#155724" : "#721c24",
                      padding: "8px",
                      textAlign: "center"
                    }
                  }, {
                    default: withCtx((_2, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(`${ssrInterpolate(item.active ? "Active" : "Not Active")}`);
                      } else {
                        return [
                          createTextVNode(toDisplayString(item.active ? "Active" : "Not Active"), 1)
                        ];
                      }
                    }),
                    _: 2
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VChip, {
                      style: {
                        backgroundColor: item.active ? "#d4edda" : "#f8d7da",
                        color: item.active ? "#155724" : "#721c24",
                        padding: "8px",
                        textAlign: "center"
                      }
                    }, {
                      default: withCtx(() => [
                        createTextVNode(toDisplayString(item.active ? "Active" : "Not Active"), 1)
                      ]),
                      _: 2
                    }, 1032, ["style"])
                  ];
                }
              }),
              "item.actions": withCtx(({ item }, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(_component_IconBtn, {
                    onClick: ($event) => handleEdit(item)
                  }, {
                    default: withCtx((_2, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VIcon, { icon: "tabler-edit" }, null, _parent4, _scopeId3));
                      } else {
                        return [
                          createVNode(VIcon, { icon: "tabler-edit" })
                        ];
                      }
                    }),
                    _: 2
                  }, _parent3, _scopeId2));
                  _push3(ssrRenderComponent(_component_IconBtn, {
                    onClick: ($event) => handleDelete(item.id),
                    color: "error"
                  }, {
                    default: withCtx((_2, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VIcon, { icon: "tabler-trash" }, null, _parent4, _scopeId3));
                      } else {
                        return [
                          createVNode(VIcon, { icon: "tabler-trash" })
                        ];
                      }
                    }),
                    _: 2
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(_component_IconBtn, {
                      onClick: ($event) => handleEdit(item)
                    }, {
                      default: withCtx(() => [
                        createVNode(VIcon, { icon: "tabler-edit" })
                      ]),
                      _: 2
                    }, 1032, ["onClick"]),
                    createVNode(_component_IconBtn, {
                      onClick: ($event) => handleDelete(item.id),
                      color: "error"
                    }, {
                      default: withCtx(() => [
                        createVNode(VIcon, { icon: "tabler-trash" })
                      ]),
                      _: 2
                    }, 1032, ["onClick"])
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
          } else {
            return [
              createVNode(VCardTitle, null, {
                default: withCtx(() => [
                  createTextVNode("Data Table")
                ]),
                _: 1
              }),
              createVNode(VRow, {
                class: "d-flex align-center",
                justify: "space-between"
              }, {
                default: withCtx(() => [
                  createVNode(VCol, { cols: "11" }, {
                    default: withCtx(() => [
                      createVNode(VTextField, {
                        modelValue: search.value,
                        "onUpdate:modelValue": ($event) => search.value = $event,
                        label: "Search",
                        class: "mb-4",
                        placeholder: "request code/request name"
                      }, null, 8, ["modelValue", "onUpdate:modelValue"])
                    ]),
                    _: 1
                  }),
                  createVNode(VCol, { cols: "1" }, {
                    default: withCtx(() => [
                      createVNode(VBtn, {
                        color: "primary",
                        onClick: openAddDialog
                      }, {
                        default: withCtx(() => [
                          createTextVNode("Add New")
                        ]),
                        _: 1
                      })
                    ]),
                    _: 1
                  })
                ]),
                _: 1
              }),
              createVNode(VDataTable, {
                headers,
                items: unref(itemDetail),
                "items-per-page": itemsPerPage.value,
                page: page.value,
                class: "elevation-1"
              }, {
                "item.no": withCtx(({ index }) => [
                  createVNode("div", { class: "text-body-1 text-high-emphasis text-capitalize" }, toDisplayString((page.value - 1) * itemsPerPage.value + 1 + index), 1)
                ]),
                "item.requestApprove": withCtx(({ item }) => [
                  createVNode(VChip, {
                    style: {
                      backgroundColor: item.requestApprove ? "#d4edda" : "#f8d7da",
                      color: item.requestApprove ? "#155724" : "#721c24",
                      padding: "8px",
                      textAlign: "center"
                    }
                  }, {
                    default: withCtx(() => [
                      createTextVNode(toDisplayString(item.requestApprove ? "Pass" : "Not Pass"), 1)
                    ]),
                    _: 2
                  }, 1032, ["style"])
                ]),
                "item.active": withCtx(({ item }) => [
                  createVNode(VChip, {
                    style: {
                      backgroundColor: item.active ? "#d4edda" : "#f8d7da",
                      color: item.active ? "#155724" : "#721c24",
                      padding: "8px",
                      textAlign: "center"
                    }
                  }, {
                    default: withCtx(() => [
                      createTextVNode(toDisplayString(item.active ? "Active" : "Not Active"), 1)
                    ]),
                    _: 2
                  }, 1032, ["style"])
                ]),
                "item.actions": withCtx(({ item }) => [
                  createVNode(_component_IconBtn, {
                    onClick: ($event) => handleEdit(item)
                  }, {
                    default: withCtx(() => [
                      createVNode(VIcon, { icon: "tabler-edit" })
                    ]),
                    _: 2
                  }, 1032, ["onClick"]),
                  createVNode(_component_IconBtn, {
                    onClick: ($event) => handleDelete(item.id),
                    color: "error"
                  }, {
                    default: withCtx(() => [
                      createVNode(VIcon, { icon: "tabler-trash" })
                    ]),
                    _: 2
                  }, 1032, ["onClick"])
                ]),
                _: 1
              }, 8, ["items", "items-per-page", "page"])
            ];
          }
        }),
        _: 1
      }, _parent));
      _push(ssrRenderComponent(_component_AddEditRequestDialog, {
        isDialogVisible: isRequestDialogVisible.value,
        "onUpdate:isDialogVisible": ($event) => isRequestDialogVisible.value = $event,
        request: unref(itemDetail),
        onSubmit
      }, null, _parent));
      _push(`<!--]-->`);
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("pages/index.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
