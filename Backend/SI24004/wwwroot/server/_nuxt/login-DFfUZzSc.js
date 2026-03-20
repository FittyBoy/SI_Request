import { V as VNodeRenderer, _ as __nuxt_component_0 } from "./VNodeRenderer-Bw61TUGP.js";
import { _ as _sfc_main$2 } from "./AppTextField-CcWZXBeD.js";
import { defineComponent, mergeProps, unref, withCtx, createVNode, useSSRContext, ref, toDisplayString, createTextVNode, withModifiers } from "vue";
import { n as useTheme, h as VBtn, V as VIcon, o as useGenerateImageVariant, t as themeConfig, f as VRow, g as VCol, p as VImg, b as VCard, d as VCardText, q as VAlert, e as VForm, r as VCheckbox, s as VDivider, v as authV2MaskDark, w as authV2MaskLight } from "../server.mjs";
import { ssrRenderAttrs, ssrRenderList, ssrRenderComponent, ssrInterpolate, ssrRenderStyle, ssrRenderAttr } from "vue/server-renderer";
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
const _sfc_main$1 = /* @__PURE__ */ defineComponent({
  __name: "AuthProvider",
  __ssrInlineRender: true,
  setup(__props) {
    const { global } = useTheme();
    const authProviders = [
      {
        icon: "tabler-brand-facebook-filled",
        color: "#4267b2",
        colorInDark: "#497CE2"
      },
      {
        icon: "tabler-brand-twitter-filled",
        color: "#1da1f2",
        colorInDark: "#1da1f2"
      },
      {
        icon: "tabler-brand-github-filled",
        color: "#272727",
        colorInDark: "#fff"
      },
      {
        icon: "tabler-brand-google-filled",
        color: "#dd4b39",
        colorInDark: "#db4437"
      }
    ];
    return (_ctx, _push, _parent, _attrs) => {
      _push(`<div${ssrRenderAttrs(mergeProps({ class: "d-flex justify-center flex-wrap gap-1" }, _attrs))}><!--[-->`);
      ssrRenderList(authProviders, (link) => {
        _push(ssrRenderComponent(VBtn, {
          key: link.icon,
          icon: "",
          variant: "text",
          size: "small",
          color: unref(global).name.value === "dark" ? link.colorInDark : link.color
        }, {
          default: withCtx((_, _push2, _parent2, _scopeId) => {
            if (_push2) {
              _push2(ssrRenderComponent(VIcon, {
                size: "20",
                icon: link.icon
              }, null, _parent2, _scopeId));
            } else {
              return [
                createVNode(VIcon, {
                  size: "20",
                  icon: link.icon
                }, null, 8, ["icon"])
              ];
            }
          }),
          _: 2
        }, _parent));
      });
      _push(`<!--]--></div>`);
    };
  }
});
const _sfc_setup$1 = _sfc_main$1.setup;
_sfc_main$1.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("views/pages/authentication/AuthProvider.vue");
  return _sfc_setup$1 ? _sfc_setup$1(props, ctx) : void 0;
};
const authV2LoginIllustrationBorderedDark = "" + __buildAssetsURL("auth-v2-login-illustration-bordered-dark.cDkPk8mY.png");
const authV2LoginIllustrationBorderedLight = "" + __buildAssetsURL("auth-v2-login-illustration-bordered-light.CIHqcIVA.png");
const authV2LoginIllustrationDark = "" + __buildAssetsURL("auth-v2-login-illustration-dark.ClExSVqL.png");
const authV2LoginIllustrationLight = "" + __buildAssetsURL("auth-v2-login-illustration-light.C4sKfRS1.png");
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "login",
  __ssrInlineRender: true,
  setup(__props) {
    const form = ref({
      email: "",
      password: "",
      remember: false
    });
    const isPasswordVisible = ref(false);
    const authThemeImg = useGenerateImageVariant(
      authV2LoginIllustrationLight,
      authV2LoginIllustrationDark,
      authV2LoginIllustrationBorderedLight,
      authV2LoginIllustrationBorderedDark,
      true
    );
    const authThemeMask = useGenerateImageVariant(authV2MaskLight, authV2MaskDark);
    return (_ctx, _push, _parent, _attrs) => {
      const _component_NuxtLink = __nuxt_component_0;
      const _component_AppTextField = _sfc_main$2;
      _push(`<!--[-->`);
      _push(ssrRenderComponent(_component_NuxtLink, { to: "/" }, {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(`<div class="auth-logo d-flex align-center gap-x-3"${_scopeId}>`);
            _push2(ssrRenderComponent(unref(VNodeRenderer), {
              nodes: unref(themeConfig).app.logo
            }, null, _parent2, _scopeId));
            _push2(`<h1 class="auth-title"${_scopeId}>${ssrInterpolate(unref(themeConfig).app.title)}</h1></div>`);
          } else {
            return [
              createVNode("div", { class: "auth-logo d-flex align-center gap-x-3" }, [
                createVNode(unref(VNodeRenderer), {
                  nodes: unref(themeConfig).app.logo
                }, null, 8, ["nodes"]),
                createVNode("h1", { class: "auth-title" }, toDisplayString(unref(themeConfig).app.title), 1)
              ])
            ];
          }
        }),
        _: 1
      }, _parent));
      _push(ssrRenderComponent(VRow, {
        "no-gutters": "",
        class: "auth-wrapper bg-surface"
      }, {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VCol, {
              md: "8",
              class: "d-none d-md-flex"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(`<div class="position-relative bg-background w-100 me-0"${_scopeId2}><div class="d-flex align-center justify-center w-100 h-100" style="${ssrRenderStyle({ "padding-inline": "6.25rem" })}"${_scopeId2}>`);
                  _push3(ssrRenderComponent(VImg, {
                    "max-width": "613",
                    src: unref(authThemeImg),
                    class: "auth-illustration mt-16 mb-2"
                  }, null, _parent3, _scopeId2));
                  _push3(`</div><img class="auth-footer-mask"${ssrRenderAttr("src", unref(authThemeMask))} alt="auth-footer-mask" height="280" width="100"${_scopeId2}></div>`);
                } else {
                  return [
                    createVNode("div", { class: "position-relative bg-background w-100 me-0" }, [
                      createVNode("div", {
                        class: "d-flex align-center justify-center w-100 h-100",
                        style: { "padding-inline": "6.25rem" }
                      }, [
                        createVNode(VImg, {
                          "max-width": "613",
                          src: unref(authThemeImg),
                          class: "auth-illustration mt-16 mb-2"
                        }, null, 8, ["src"])
                      ]),
                      createVNode("img", {
                        class: "auth-footer-mask",
                        src: unref(authThemeMask),
                        alt: "auth-footer-mask",
                        height: "280",
                        width: "100"
                      }, null, 8, ["src"])
                    ])
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
            _push2(ssrRenderComponent(VCol, {
              cols: "12",
              md: "4",
              class: "auth-card-v2 d-flex align-center justify-center"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VCard, {
                    flat: "",
                    "max-width": 500,
                    class: "mt-12 mt-sm-0 pa-4"
                  }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VCardText, null, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(`<h4 class="text-h4 mb-1"${_scopeId4}> Welcome to <span class="text-capitalize"${_scopeId4}>${ssrInterpolate(unref(themeConfig).app.title)}</span>! 👋🏻 </h4><p class="mb-0"${_scopeId4}> Please sign-in to your account and start the adventure </p>`);
                            } else {
                              return [
                                createVNode("h4", { class: "text-h4 mb-1" }, [
                                  createTextVNode(" Welcome to "),
                                  createVNode("span", { class: "text-capitalize" }, toDisplayString(unref(themeConfig).app.title), 1),
                                  createTextVNode("! 👋🏻 ")
                                ]),
                                createVNode("p", { class: "mb-0" }, " Please sign-in to your account and start the adventure ")
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                        _push4(ssrRenderComponent(VCardText, null, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VAlert, {
                                color: "primary",
                                variant: "tonal"
                              }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(`<p class="text-sm mb-2"${_scopeId5}> Admin Email: <strong${_scopeId5}>admin@demo.com</strong> / Pass: <strong${_scopeId5}>admin</strong></p><p class="text-sm mb-0"${_scopeId5}> Client Email: <strong${_scopeId5}>client@demo.com</strong> / Pass: <strong${_scopeId5}>client</strong></p>`);
                                  } else {
                                    return [
                                      createVNode("p", { class: "text-sm mb-2" }, [
                                        createTextVNode(" Admin Email: "),
                                        createVNode("strong", null, "admin@demo.com"),
                                        createTextVNode(" / Pass: "),
                                        createVNode("strong", null, "admin")
                                      ]),
                                      createVNode("p", { class: "text-sm mb-0" }, [
                                        createTextVNode(" Client Email: "),
                                        createVNode("strong", null, "client@demo.com"),
                                        createTextVNode(" / Pass: "),
                                        createVNode("strong", null, "client")
                                      ])
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                            } else {
                              return [
                                createVNode(VAlert, {
                                  color: "primary",
                                  variant: "tonal"
                                }, {
                                  default: withCtx(() => [
                                    createVNode("p", { class: "text-sm mb-2" }, [
                                      createTextVNode(" Admin Email: "),
                                      createVNode("strong", null, "admin@demo.com"),
                                      createTextVNode(" / Pass: "),
                                      createVNode("strong", null, "admin")
                                    ]),
                                    createVNode("p", { class: "text-sm mb-0" }, [
                                      createTextVNode(" Client Email: "),
                                      createVNode("strong", null, "client@demo.com"),
                                      createTextVNode(" / Pass: "),
                                      createVNode("strong", null, "client")
                                    ])
                                  ]),
                                  _: 1
                                })
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                        _push4(ssrRenderComponent(VCardText, null, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VForm, { onSubmit: () => {
                              } }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VRow, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(ssrRenderComponent(VCol, { cols: "12" }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(ssrRenderComponent(_component_AppTextField, {
                                                  modelValue: unref(form).email,
                                                  "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                                  autofocus: "",
                                                  label: "Email",
                                                  type: "email",
                                                  placeholder: "johndoe@email.com"
                                                }, null, _parent8, _scopeId7));
                                              } else {
                                                return [
                                                  createVNode(_component_AppTextField, {
                                                    modelValue: unref(form).email,
                                                    "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                                    autofocus: "",
                                                    label: "Email",
                                                    type: "email",
                                                    placeholder: "johndoe@email.com"
                                                  }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                          _push7(ssrRenderComponent(VCol, { cols: "12" }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(ssrRenderComponent(_component_AppTextField, {
                                                  modelValue: unref(form).password,
                                                  "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                                  label: "Password",
                                                  placeholder: "············",
                                                  type: unref(isPasswordVisible) ? "text" : "password",
                                                  "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                                  "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                                }, null, _parent8, _scopeId7));
                                                _push8(`<div class="d-flex align-center flex-wrap justify-space-between mt-2 mb-4"${_scopeId7}>`);
                                                _push8(ssrRenderComponent(VCheckbox, {
                                                  modelValue: unref(form).remember,
                                                  "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                                  label: "Remember me"
                                                }, null, _parent8, _scopeId7));
                                                _push8(`<a class="text-primary ms-2 mb-1" href="#"${_scopeId7}> Forgot Password? </a></div>`);
                                                _push8(ssrRenderComponent(VBtn, {
                                                  block: "",
                                                  type: "submit"
                                                }, {
                                                  default: withCtx((_8, _push9, _parent9, _scopeId8) => {
                                                    if (_push9) {
                                                      _push9(` Login `);
                                                    } else {
                                                      return [
                                                        createTextVNode(" Login ")
                                                      ];
                                                    }
                                                  }),
                                                  _: 1
                                                }, _parent8, _scopeId7));
                                              } else {
                                                return [
                                                  createVNode(_component_AppTextField, {
                                                    modelValue: unref(form).password,
                                                    "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                                    label: "Password",
                                                    placeholder: "············",
                                                    type: unref(isPasswordVisible) ? "text" : "password",
                                                    "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                                    "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                                  }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                                  createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                                    createVNode(VCheckbox, {
                                                      modelValue: unref(form).remember,
                                                      "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                                      label: "Remember me"
                                                    }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                                    createVNode("a", {
                                                      class: "text-primary ms-2 mb-1",
                                                      href: "#"
                                                    }, " Forgot Password? ")
                                                  ]),
                                                  createVNode(VBtn, {
                                                    block: "",
                                                    type: "submit"
                                                  }, {
                                                    default: withCtx(() => [
                                                      createTextVNode(" Login ")
                                                    ]),
                                                    _: 1
                                                  })
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                          _push7(ssrRenderComponent(VCol, {
                                            cols: "12",
                                            class: "text-center"
                                          }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(`<span${_scopeId7}>New on our platform?</span><a class="text-primary ms-2" href="#"${_scopeId7}> Create an account </a>`);
                                              } else {
                                                return [
                                                  createVNode("span", null, "New on our platform?"),
                                                  createVNode("a", {
                                                    class: "text-primary ms-2",
                                                    href: "#"
                                                  }, " Create an account ")
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                          _push7(ssrRenderComponent(VCol, {
                                            cols: "12",
                                            class: "d-flex align-center"
                                          }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(ssrRenderComponent(VDivider, null, null, _parent8, _scopeId7));
                                                _push8(`<span class="mx-4"${_scopeId7}>or</span>`);
                                                _push8(ssrRenderComponent(VDivider, null, null, _parent8, _scopeId7));
                                              } else {
                                                return [
                                                  createVNode(VDivider),
                                                  createVNode("span", { class: "mx-4" }, "or"),
                                                  createVNode(VDivider)
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                          _push7(ssrRenderComponent(VCol, {
                                            cols: "12",
                                            class: "text-center"
                                          }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(ssrRenderComponent(_sfc_main$1, null, null, _parent8, _scopeId7));
                                              } else {
                                                return [
                                                  createVNode(_sfc_main$1)
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                        } else {
                                          return [
                                            createVNode(VCol, { cols: "12" }, {
                                              default: withCtx(() => [
                                                createVNode(_component_AppTextField, {
                                                  modelValue: unref(form).email,
                                                  "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                                  autofocus: "",
                                                  label: "Email",
                                                  type: "email",
                                                  placeholder: "johndoe@email.com"
                                                }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                              ]),
                                              _: 1
                                            }),
                                            createVNode(VCol, { cols: "12" }, {
                                              default: withCtx(() => [
                                                createVNode(_component_AppTextField, {
                                                  modelValue: unref(form).password,
                                                  "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                                  label: "Password",
                                                  placeholder: "············",
                                                  type: unref(isPasswordVisible) ? "text" : "password",
                                                  "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                                  "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                                }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                                createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                                  createVNode(VCheckbox, {
                                                    modelValue: unref(form).remember,
                                                    "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                                    label: "Remember me"
                                                  }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                                  createVNode("a", {
                                                    class: "text-primary ms-2 mb-1",
                                                    href: "#"
                                                  }, " Forgot Password? ")
                                                ]),
                                                createVNode(VBtn, {
                                                  block: "",
                                                  type: "submit"
                                                }, {
                                                  default: withCtx(() => [
                                                    createTextVNode(" Login ")
                                                  ]),
                                                  _: 1
                                                })
                                              ]),
                                              _: 1
                                            }),
                                            createVNode(VCol, {
                                              cols: "12",
                                              class: "text-center"
                                            }, {
                                              default: withCtx(() => [
                                                createVNode("span", null, "New on our platform?"),
                                                createVNode("a", {
                                                  class: "text-primary ms-2",
                                                  href: "#"
                                                }, " Create an account ")
                                              ]),
                                              _: 1
                                            }),
                                            createVNode(VCol, {
                                              cols: "12",
                                              class: "d-flex align-center"
                                            }, {
                                              default: withCtx(() => [
                                                createVNode(VDivider),
                                                createVNode("span", { class: "mx-4" }, "or"),
                                                createVNode(VDivider)
                                              ]),
                                              _: 1
                                            }),
                                            createVNode(VCol, {
                                              cols: "12",
                                              class: "text-center"
                                            }, {
                                              default: withCtx(() => [
                                                createVNode(_sfc_main$1)
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
                                      createVNode(VRow, null, {
                                        default: withCtx(() => [
                                          createVNode(VCol, { cols: "12" }, {
                                            default: withCtx(() => [
                                              createVNode(_component_AppTextField, {
                                                modelValue: unref(form).email,
                                                "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                                autofocus: "",
                                                label: "Email",
                                                type: "email",
                                                placeholder: "johndoe@email.com"
                                              }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                            ]),
                                            _: 1
                                          }),
                                          createVNode(VCol, { cols: "12" }, {
                                            default: withCtx(() => [
                                              createVNode(_component_AppTextField, {
                                                modelValue: unref(form).password,
                                                "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                                label: "Password",
                                                placeholder: "············",
                                                type: unref(isPasswordVisible) ? "text" : "password",
                                                "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                                "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                              }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                              createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                                createVNode(VCheckbox, {
                                                  modelValue: unref(form).remember,
                                                  "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                                  label: "Remember me"
                                                }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                                createVNode("a", {
                                                  class: "text-primary ms-2 mb-1",
                                                  href: "#"
                                                }, " Forgot Password? ")
                                              ]),
                                              createVNode(VBtn, {
                                                block: "",
                                                type: "submit"
                                              }, {
                                                default: withCtx(() => [
                                                  createTextVNode(" Login ")
                                                ]),
                                                _: 1
                                              })
                                            ]),
                                            _: 1
                                          }),
                                          createVNode(VCol, {
                                            cols: "12",
                                            class: "text-center"
                                          }, {
                                            default: withCtx(() => [
                                              createVNode("span", null, "New on our platform?"),
                                              createVNode("a", {
                                                class: "text-primary ms-2",
                                                href: "#"
                                              }, " Create an account ")
                                            ]),
                                            _: 1
                                          }),
                                          createVNode(VCol, {
                                            cols: "12",
                                            class: "d-flex align-center"
                                          }, {
                                            default: withCtx(() => [
                                              createVNode(VDivider),
                                              createVNode("span", { class: "mx-4" }, "or"),
                                              createVNode(VDivider)
                                            ]),
                                            _: 1
                                          }),
                                          createVNode(VCol, {
                                            cols: "12",
                                            class: "text-center"
                                          }, {
                                            default: withCtx(() => [
                                              createVNode(_sfc_main$1)
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
                                createVNode(VForm, {
                                  onSubmit: withModifiers(() => {
                                  }, ["prevent"])
                                }, {
                                  default: withCtx(() => [
                                    createVNode(VRow, null, {
                                      default: withCtx(() => [
                                        createVNode(VCol, { cols: "12" }, {
                                          default: withCtx(() => [
                                            createVNode(_component_AppTextField, {
                                              modelValue: unref(form).email,
                                              "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                              autofocus: "",
                                              label: "Email",
                                              type: "email",
                                              placeholder: "johndoe@email.com"
                                            }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                          ]),
                                          _: 1
                                        }),
                                        createVNode(VCol, { cols: "12" }, {
                                          default: withCtx(() => [
                                            createVNode(_component_AppTextField, {
                                              modelValue: unref(form).password,
                                              "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                              label: "Password",
                                              placeholder: "············",
                                              type: unref(isPasswordVisible) ? "text" : "password",
                                              "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                              "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                            }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                            createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                              createVNode(VCheckbox, {
                                                modelValue: unref(form).remember,
                                                "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                                label: "Remember me"
                                              }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                              createVNode("a", {
                                                class: "text-primary ms-2 mb-1",
                                                href: "#"
                                              }, " Forgot Password? ")
                                            ]),
                                            createVNode(VBtn, {
                                              block: "",
                                              type: "submit"
                                            }, {
                                              default: withCtx(() => [
                                                createTextVNode(" Login ")
                                              ]),
                                              _: 1
                                            })
                                          ]),
                                          _: 1
                                        }),
                                        createVNode(VCol, {
                                          cols: "12",
                                          class: "text-center"
                                        }, {
                                          default: withCtx(() => [
                                            createVNode("span", null, "New on our platform?"),
                                            createVNode("a", {
                                              class: "text-primary ms-2",
                                              href: "#"
                                            }, " Create an account ")
                                          ]),
                                          _: 1
                                        }),
                                        createVNode(VCol, {
                                          cols: "12",
                                          class: "d-flex align-center"
                                        }, {
                                          default: withCtx(() => [
                                            createVNode(VDivider),
                                            createVNode("span", { class: "mx-4" }, "or"),
                                            createVNode(VDivider)
                                          ]),
                                          _: 1
                                        }),
                                        createVNode(VCol, {
                                          cols: "12",
                                          class: "text-center"
                                        }, {
                                          default: withCtx(() => [
                                            createVNode(_sfc_main$1)
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
                          createVNode(VCardText, null, {
                            default: withCtx(() => [
                              createVNode("h4", { class: "text-h4 mb-1" }, [
                                createTextVNode(" Welcome to "),
                                createVNode("span", { class: "text-capitalize" }, toDisplayString(unref(themeConfig).app.title), 1),
                                createTextVNode("! 👋🏻 ")
                              ]),
                              createVNode("p", { class: "mb-0" }, " Please sign-in to your account and start the adventure ")
                            ]),
                            _: 1
                          }),
                          createVNode(VCardText, null, {
                            default: withCtx(() => [
                              createVNode(VAlert, {
                                color: "primary",
                                variant: "tonal"
                              }, {
                                default: withCtx(() => [
                                  createVNode("p", { class: "text-sm mb-2" }, [
                                    createTextVNode(" Admin Email: "),
                                    createVNode("strong", null, "admin@demo.com"),
                                    createTextVNode(" / Pass: "),
                                    createVNode("strong", null, "admin")
                                  ]),
                                  createVNode("p", { class: "text-sm mb-0" }, [
                                    createTextVNode(" Client Email: "),
                                    createVNode("strong", null, "client@demo.com"),
                                    createTextVNode(" / Pass: "),
                                    createVNode("strong", null, "client")
                                  ])
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VCardText, null, {
                            default: withCtx(() => [
                              createVNode(VForm, {
                                onSubmit: withModifiers(() => {
                                }, ["prevent"])
                              }, {
                                default: withCtx(() => [
                                  createVNode(VRow, null, {
                                    default: withCtx(() => [
                                      createVNode(VCol, { cols: "12" }, {
                                        default: withCtx(() => [
                                          createVNode(_component_AppTextField, {
                                            modelValue: unref(form).email,
                                            "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                            autofocus: "",
                                            label: "Email",
                                            type: "email",
                                            placeholder: "johndoe@email.com"
                                          }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VCol, { cols: "12" }, {
                                        default: withCtx(() => [
                                          createVNode(_component_AppTextField, {
                                            modelValue: unref(form).password,
                                            "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                            label: "Password",
                                            placeholder: "············",
                                            type: unref(isPasswordVisible) ? "text" : "password",
                                            "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                            "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                          }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                          createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                            createVNode(VCheckbox, {
                                              modelValue: unref(form).remember,
                                              "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                              label: "Remember me"
                                            }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                            createVNode("a", {
                                              class: "text-primary ms-2 mb-1",
                                              href: "#"
                                            }, " Forgot Password? ")
                                          ]),
                                          createVNode(VBtn, {
                                            block: "",
                                            type: "submit"
                                          }, {
                                            default: withCtx(() => [
                                              createTextVNode(" Login ")
                                            ]),
                                            _: 1
                                          })
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VCol, {
                                        cols: "12",
                                        class: "text-center"
                                      }, {
                                        default: withCtx(() => [
                                          createVNode("span", null, "New on our platform?"),
                                          createVNode("a", {
                                            class: "text-primary ms-2",
                                            href: "#"
                                          }, " Create an account ")
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VCol, {
                                        cols: "12",
                                        class: "d-flex align-center"
                                      }, {
                                        default: withCtx(() => [
                                          createVNode(VDivider),
                                          createVNode("span", { class: "mx-4" }, "or"),
                                          createVNode(VDivider)
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VCol, {
                                        cols: "12",
                                        class: "text-center"
                                      }, {
                                        default: withCtx(() => [
                                          createVNode(_sfc_main$1)
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
                          })
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VCard, {
                      flat: "",
                      "max-width": 500,
                      class: "mt-12 mt-sm-0 pa-4"
                    }, {
                      default: withCtx(() => [
                        createVNode(VCardText, null, {
                          default: withCtx(() => [
                            createVNode("h4", { class: "text-h4 mb-1" }, [
                              createTextVNode(" Welcome to "),
                              createVNode("span", { class: "text-capitalize" }, toDisplayString(unref(themeConfig).app.title), 1),
                              createTextVNode("! 👋🏻 ")
                            ]),
                            createVNode("p", { class: "mb-0" }, " Please sign-in to your account and start the adventure ")
                          ]),
                          _: 1
                        }),
                        createVNode(VCardText, null, {
                          default: withCtx(() => [
                            createVNode(VAlert, {
                              color: "primary",
                              variant: "tonal"
                            }, {
                              default: withCtx(() => [
                                createVNode("p", { class: "text-sm mb-2" }, [
                                  createTextVNode(" Admin Email: "),
                                  createVNode("strong", null, "admin@demo.com"),
                                  createTextVNode(" / Pass: "),
                                  createVNode("strong", null, "admin")
                                ]),
                                createVNode("p", { class: "text-sm mb-0" }, [
                                  createTextVNode(" Client Email: "),
                                  createVNode("strong", null, "client@demo.com"),
                                  createTextVNode(" / Pass: "),
                                  createVNode("strong", null, "client")
                                ])
                              ]),
                              _: 1
                            })
                          ]),
                          _: 1
                        }),
                        createVNode(VCardText, null, {
                          default: withCtx(() => [
                            createVNode(VForm, {
                              onSubmit: withModifiers(() => {
                              }, ["prevent"])
                            }, {
                              default: withCtx(() => [
                                createVNode(VRow, null, {
                                  default: withCtx(() => [
                                    createVNode(VCol, { cols: "12" }, {
                                      default: withCtx(() => [
                                        createVNode(_component_AppTextField, {
                                          modelValue: unref(form).email,
                                          "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                          autofocus: "",
                                          label: "Email",
                                          type: "email",
                                          placeholder: "johndoe@email.com"
                                        }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VCol, { cols: "12" }, {
                                      default: withCtx(() => [
                                        createVNode(_component_AppTextField, {
                                          modelValue: unref(form).password,
                                          "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                          label: "Password",
                                          placeholder: "············",
                                          type: unref(isPasswordVisible) ? "text" : "password",
                                          "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                          "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                        }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                        createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                          createVNode(VCheckbox, {
                                            modelValue: unref(form).remember,
                                            "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                            label: "Remember me"
                                          }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                          createVNode("a", {
                                            class: "text-primary ms-2 mb-1",
                                            href: "#"
                                          }, " Forgot Password? ")
                                        ]),
                                        createVNode(VBtn, {
                                          block: "",
                                          type: "submit"
                                        }, {
                                          default: withCtx(() => [
                                            createTextVNode(" Login ")
                                          ]),
                                          _: 1
                                        })
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VCol, {
                                      cols: "12",
                                      class: "text-center"
                                    }, {
                                      default: withCtx(() => [
                                        createVNode("span", null, "New on our platform?"),
                                        createVNode("a", {
                                          class: "text-primary ms-2",
                                          href: "#"
                                        }, " Create an account ")
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VCol, {
                                      cols: "12",
                                      class: "d-flex align-center"
                                    }, {
                                      default: withCtx(() => [
                                        createVNode(VDivider),
                                        createVNode("span", { class: "mx-4" }, "or"),
                                        createVNode(VDivider)
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VCol, {
                                      cols: "12",
                                      class: "text-center"
                                    }, {
                                      default: withCtx(() => [
                                        createVNode(_sfc_main$1)
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
                        })
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
              createVNode(VCol, {
                md: "8",
                class: "d-none d-md-flex"
              }, {
                default: withCtx(() => [
                  createVNode("div", { class: "position-relative bg-background w-100 me-0" }, [
                    createVNode("div", {
                      class: "d-flex align-center justify-center w-100 h-100",
                      style: { "padding-inline": "6.25rem" }
                    }, [
                      createVNode(VImg, {
                        "max-width": "613",
                        src: unref(authThemeImg),
                        class: "auth-illustration mt-16 mb-2"
                      }, null, 8, ["src"])
                    ]),
                    createVNode("img", {
                      class: "auth-footer-mask",
                      src: unref(authThemeMask),
                      alt: "auth-footer-mask",
                      height: "280",
                      width: "100"
                    }, null, 8, ["src"])
                  ])
                ]),
                _: 1
              }),
              createVNode(VCol, {
                cols: "12",
                md: "4",
                class: "auth-card-v2 d-flex align-center justify-center"
              }, {
                default: withCtx(() => [
                  createVNode(VCard, {
                    flat: "",
                    "max-width": 500,
                    class: "mt-12 mt-sm-0 pa-4"
                  }, {
                    default: withCtx(() => [
                      createVNode(VCardText, null, {
                        default: withCtx(() => [
                          createVNode("h4", { class: "text-h4 mb-1" }, [
                            createTextVNode(" Welcome to "),
                            createVNode("span", { class: "text-capitalize" }, toDisplayString(unref(themeConfig).app.title), 1),
                            createTextVNode("! 👋🏻 ")
                          ]),
                          createVNode("p", { class: "mb-0" }, " Please sign-in to your account and start the adventure ")
                        ]),
                        _: 1
                      }),
                      createVNode(VCardText, null, {
                        default: withCtx(() => [
                          createVNode(VAlert, {
                            color: "primary",
                            variant: "tonal"
                          }, {
                            default: withCtx(() => [
                              createVNode("p", { class: "text-sm mb-2" }, [
                                createTextVNode(" Admin Email: "),
                                createVNode("strong", null, "admin@demo.com"),
                                createTextVNode(" / Pass: "),
                                createVNode("strong", null, "admin")
                              ]),
                              createVNode("p", { class: "text-sm mb-0" }, [
                                createTextVNode(" Client Email: "),
                                createVNode("strong", null, "client@demo.com"),
                                createTextVNode(" / Pass: "),
                                createVNode("strong", null, "client")
                              ])
                            ]),
                            _: 1
                          })
                        ]),
                        _: 1
                      }),
                      createVNode(VCardText, null, {
                        default: withCtx(() => [
                          createVNode(VForm, {
                            onSubmit: withModifiers(() => {
                            }, ["prevent"])
                          }, {
                            default: withCtx(() => [
                              createVNode(VRow, null, {
                                default: withCtx(() => [
                                  createVNode(VCol, { cols: "12" }, {
                                    default: withCtx(() => [
                                      createVNode(_component_AppTextField, {
                                        modelValue: unref(form).email,
                                        "onUpdate:modelValue": ($event) => unref(form).email = $event,
                                        autofocus: "",
                                        label: "Email",
                                        type: "email",
                                        placeholder: "johndoe@email.com"
                                      }, null, 8, ["modelValue", "onUpdate:modelValue"])
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VCol, { cols: "12" }, {
                                    default: withCtx(() => [
                                      createVNode(_component_AppTextField, {
                                        modelValue: unref(form).password,
                                        "onUpdate:modelValue": ($event) => unref(form).password = $event,
                                        label: "Password",
                                        placeholder: "············",
                                        type: unref(isPasswordVisible) ? "text" : "password",
                                        "append-inner-icon": unref(isPasswordVisible) ? "tabler-eye-off" : "tabler-eye",
                                        "onClick:appendInner": ($event) => isPasswordVisible.value = !unref(isPasswordVisible)
                                      }, null, 8, ["modelValue", "onUpdate:modelValue", "type", "append-inner-icon", "onClick:appendInner"]),
                                      createVNode("div", { class: "d-flex align-center flex-wrap justify-space-between mt-2 mb-4" }, [
                                        createVNode(VCheckbox, {
                                          modelValue: unref(form).remember,
                                          "onUpdate:modelValue": ($event) => unref(form).remember = $event,
                                          label: "Remember me"
                                        }, null, 8, ["modelValue", "onUpdate:modelValue"]),
                                        createVNode("a", {
                                          class: "text-primary ms-2 mb-1",
                                          href: "#"
                                        }, " Forgot Password? ")
                                      ]),
                                      createVNode(VBtn, {
                                        block: "",
                                        type: "submit"
                                      }, {
                                        default: withCtx(() => [
                                          createTextVNode(" Login ")
                                        ]),
                                        _: 1
                                      })
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VCol, {
                                    cols: "12",
                                    class: "text-center"
                                  }, {
                                    default: withCtx(() => [
                                      createVNode("span", null, "New on our platform?"),
                                      createVNode("a", {
                                        class: "text-primary ms-2",
                                        href: "#"
                                      }, " Create an account ")
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VCol, {
                                    cols: "12",
                                    class: "d-flex align-center"
                                  }, {
                                    default: withCtx(() => [
                                      createVNode(VDivider),
                                      createVNode("span", { class: "mx-4" }, "or"),
                                      createVNode(VDivider)
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VCol, {
                                    cols: "12",
                                    class: "text-center"
                                  }, {
                                    default: withCtx(() => [
                                      createVNode(_sfc_main$1)
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
      }, _parent));
      _push(`<!--]-->`);
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("pages/login.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
