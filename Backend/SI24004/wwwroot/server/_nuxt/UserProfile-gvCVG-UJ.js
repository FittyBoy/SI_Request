import { defineComponent, mergeProps, withCtx, unref, createVNode, createTextVNode, useSSRContext } from "vue";
import { ssrRenderComponent } from "vue/server-renderer";
import { Y as VBadge, Z as VAvatar, p as VImg, U as VMenu, W as VList, X as VListItem, $ as VListItemAction, a0 as VListItemTitle, a1 as VListItemSubtitle, s as VDivider, V as VIcon } from "../server.mjs";
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
const avatar1 = "" + __buildAssetsURL("avatar-1.DMk2FF1-.png");
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "UserProfile",
  __ssrInlineRender: true,
  setup(__props) {
    return (_ctx, _push, _parent, _attrs) => {
      _push(ssrRenderComponent(VBadge, mergeProps({
        dot: "",
        location: "bottom right",
        "offset-x": "3",
        "offset-y": "3",
        bordered: "",
        color: "success"
      }, _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VAvatar, {
              class: "cursor-pointer",
              color: "primary",
              variant: "tonal"
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VImg, { src: unref(avatar1) }, null, _parent3, _scopeId2));
                  _push3(ssrRenderComponent(VMenu, {
                    activator: "parent",
                    width: "230",
                    location: "bottom end",
                    offset: "14px"
                  }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VList, null, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VListItem, null, {
                                prepend: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemAction, { start: "" }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(ssrRenderComponent(VBadge, {
                                            dot: "",
                                            location: "bottom right",
                                            "offset-x": "3",
                                            "offset-y": "3",
                                            color: "success"
                                          }, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(ssrRenderComponent(VAvatar, {
                                                  color: "primary",
                                                  variant: "tonal"
                                                }, {
                                                  default: withCtx((_8, _push9, _parent9, _scopeId8) => {
                                                    if (_push9) {
                                                      _push9(ssrRenderComponent(VImg, { src: unref(avatar1) }, null, _parent9, _scopeId8));
                                                    } else {
                                                      return [
                                                        createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
                                                      ];
                                                    }
                                                  }),
                                                  _: 1
                                                }, _parent8, _scopeId7));
                                              } else {
                                                return [
                                                  createVNode(VAvatar, {
                                                    color: "primary",
                                                    variant: "tonal"
                                                  }, {
                                                    default: withCtx(() => [
                                                      createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
                                                    ]),
                                                    _: 1
                                                  })
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                        } else {
                                          return [
                                            createVNode(VBadge, {
                                              dot: "",
                                              location: "bottom right",
                                              "offset-x": "3",
                                              "offset-y": "3",
                                              color: "success"
                                            }, {
                                              default: withCtx(() => [
                                                createVNode(VAvatar, {
                                                  color: "primary",
                                                  variant: "tonal"
                                                }, {
                                                  default: withCtx(() => [
                                                    createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
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
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemAction, { start: "" }, {
                                        default: withCtx(() => [
                                          createVNode(VBadge, {
                                            dot: "",
                                            location: "bottom right",
                                            "offset-x": "3",
                                            "offset-y": "3",
                                            color: "success"
                                          }, {
                                            default: withCtx(() => [
                                              createVNode(VAvatar, {
                                                color: "primary",
                                                variant: "tonal"
                                              }, {
                                                default: withCtx(() => [
                                                  createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
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
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemTitle, { class: "font-weight-semibold" }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(` John Doe `);
                                        } else {
                                          return [
                                            createTextVNode(" John Doe ")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                    _push6(ssrRenderComponent(VListItemSubtitle, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`Admin`);
                                        } else {
                                          return [
                                            createTextVNode("Admin")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemTitle, { class: "font-weight-semibold" }, {
                                        default: withCtx(() => [
                                          createTextVNode(" John Doe ")
                                        ]),
                                        _: 1
                                      }),
                                      createVNode(VListItemSubtitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("Admin")
                                        ]),
                                        _: 1
                                      })
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VDivider, { class: "my-2" }, null, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VListItem, { link: "" }, {
                                prepend: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-user",
                                      size: "22"
                                    }, null, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VIcon, {
                                        class: "me-2",
                                        icon: "tabler-user",
                                        size: "22"
                                      })
                                    ];
                                  }
                                }),
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemTitle, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`Profile`);
                                        } else {
                                          return [
                                            createTextVNode("Profile")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("Profile")
                                        ]),
                                        _: 1
                                      })
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VListItem, { link: "" }, {
                                prepend: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-settings",
                                      size: "22"
                                    }, null, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VIcon, {
                                        class: "me-2",
                                        icon: "tabler-settings",
                                        size: "22"
                                      })
                                    ];
                                  }
                                }),
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemTitle, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`Settings`);
                                        } else {
                                          return [
                                            createTextVNode("Settings")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("Settings")
                                        ]),
                                        _: 1
                                      })
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VListItem, { link: "" }, {
                                prepend: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-currency-dollar",
                                      size: "22"
                                    }, null, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VIcon, {
                                        class: "me-2",
                                        icon: "tabler-currency-dollar",
                                        size: "22"
                                      })
                                    ];
                                  }
                                }),
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemTitle, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`Pricing`);
                                        } else {
                                          return [
                                            createTextVNode("Pricing")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("Pricing")
                                        ]),
                                        _: 1
                                      })
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VListItem, { link: "" }, {
                                prepend: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-help",
                                      size: "22"
                                    }, null, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VIcon, {
                                        class: "me-2",
                                        icon: "tabler-help",
                                        size: "22"
                                      })
                                    ];
                                  }
                                }),
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemTitle, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`FAQ`);
                                        } else {
                                          return [
                                            createTextVNode("FAQ")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("FAQ")
                                        ]),
                                        _: 1
                                      })
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VDivider, { class: "my-2" }, null, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(VListItem, { to: "/login" }, {
                                prepend: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-logout",
                                      size: "22"
                                    }, null, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VIcon, {
                                        class: "me-2",
                                        icon: "tabler-logout",
                                        size: "22"
                                      })
                                    ];
                                  }
                                }),
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VListItemTitle, null, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`Logout`);
                                        } else {
                                          return [
                                            createTextVNode("Logout")
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("Logout")
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
                                createVNode(VListItem, null, {
                                  prepend: withCtx(() => [
                                    createVNode(VListItemAction, { start: "" }, {
                                      default: withCtx(() => [
                                        createVNode(VBadge, {
                                          dot: "",
                                          location: "bottom right",
                                          "offset-x": "3",
                                          "offset-y": "3",
                                          color: "success"
                                        }, {
                                          default: withCtx(() => [
                                            createVNode(VAvatar, {
                                              color: "primary",
                                              variant: "tonal"
                                            }, {
                                              default: withCtx(() => [
                                                createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
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
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, { class: "font-weight-semibold" }, {
                                      default: withCtx(() => [
                                        createTextVNode(" John Doe ")
                                      ]),
                                      _: 1
                                    }),
                                    createVNode(VListItemSubtitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("Admin")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }),
                                createVNode(VDivider, { class: "my-2" }),
                                createVNode(VListItem, { link: "" }, {
                                  prepend: withCtx(() => [
                                    createVNode(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-user",
                                      size: "22"
                                    })
                                  ]),
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("Profile")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }),
                                createVNode(VListItem, { link: "" }, {
                                  prepend: withCtx(() => [
                                    createVNode(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-settings",
                                      size: "22"
                                    })
                                  ]),
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("Settings")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }),
                                createVNode(VListItem, { link: "" }, {
                                  prepend: withCtx(() => [
                                    createVNode(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-currency-dollar",
                                      size: "22"
                                    })
                                  ]),
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("Pricing")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }),
                                createVNode(VListItem, { link: "" }, {
                                  prepend: withCtx(() => [
                                    createVNode(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-help",
                                      size: "22"
                                    })
                                  ]),
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("FAQ")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }),
                                createVNode(VDivider, { class: "my-2" }),
                                createVNode(VListItem, { to: "/login" }, {
                                  prepend: withCtx(() => [
                                    createVNode(VIcon, {
                                      class: "me-2",
                                      icon: "tabler-logout",
                                      size: "22"
                                    })
                                  ]),
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("Logout")
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
                          createVNode(VList, null, {
                            default: withCtx(() => [
                              createVNode(VListItem, null, {
                                prepend: withCtx(() => [
                                  createVNode(VListItemAction, { start: "" }, {
                                    default: withCtx(() => [
                                      createVNode(VBadge, {
                                        dot: "",
                                        location: "bottom right",
                                        "offset-x": "3",
                                        "offset-y": "3",
                                        color: "success"
                                      }, {
                                        default: withCtx(() => [
                                          createVNode(VAvatar, {
                                            color: "primary",
                                            variant: "tonal"
                                          }, {
                                            default: withCtx(() => [
                                              createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
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
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, { class: "font-weight-semibold" }, {
                                    default: withCtx(() => [
                                      createTextVNode(" John Doe ")
                                    ]),
                                    _: 1
                                  }),
                                  createVNode(VListItemSubtitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("Admin")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }),
                              createVNode(VDivider, { class: "my-2" }),
                              createVNode(VListItem, { link: "" }, {
                                prepend: withCtx(() => [
                                  createVNode(VIcon, {
                                    class: "me-2",
                                    icon: "tabler-user",
                                    size: "22"
                                  })
                                ]),
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("Profile")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }),
                              createVNode(VListItem, { link: "" }, {
                                prepend: withCtx(() => [
                                  createVNode(VIcon, {
                                    class: "me-2",
                                    icon: "tabler-settings",
                                    size: "22"
                                  })
                                ]),
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("Settings")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }),
                              createVNode(VListItem, { link: "" }, {
                                prepend: withCtx(() => [
                                  createVNode(VIcon, {
                                    class: "me-2",
                                    icon: "tabler-currency-dollar",
                                    size: "22"
                                  })
                                ]),
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("Pricing")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }),
                              createVNode(VListItem, { link: "" }, {
                                prepend: withCtx(() => [
                                  createVNode(VIcon, {
                                    class: "me-2",
                                    icon: "tabler-help",
                                    size: "22"
                                  })
                                ]),
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("FAQ")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }),
                              createVNode(VDivider, { class: "my-2" }),
                              createVNode(VListItem, { to: "/login" }, {
                                prepend: withCtx(() => [
                                  createVNode(VIcon, {
                                    class: "me-2",
                                    icon: "tabler-logout",
                                    size: "22"
                                  })
                                ]),
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("Logout")
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
                    createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"]),
                    createVNode(VMenu, {
                      activator: "parent",
                      width: "230",
                      location: "bottom end",
                      offset: "14px"
                    }, {
                      default: withCtx(() => [
                        createVNode(VList, null, {
                          default: withCtx(() => [
                            createVNode(VListItem, null, {
                              prepend: withCtx(() => [
                                createVNode(VListItemAction, { start: "" }, {
                                  default: withCtx(() => [
                                    createVNode(VBadge, {
                                      dot: "",
                                      location: "bottom right",
                                      "offset-x": "3",
                                      "offset-y": "3",
                                      color: "success"
                                    }, {
                                      default: withCtx(() => [
                                        createVNode(VAvatar, {
                                          color: "primary",
                                          variant: "tonal"
                                        }, {
                                          default: withCtx(() => [
                                            createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
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
                              default: withCtx(() => [
                                createVNode(VListItemTitle, { class: "font-weight-semibold" }, {
                                  default: withCtx(() => [
                                    createTextVNode(" John Doe ")
                                  ]),
                                  _: 1
                                }),
                                createVNode(VListItemSubtitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode("Admin")
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            }),
                            createVNode(VDivider, { class: "my-2" }),
                            createVNode(VListItem, { link: "" }, {
                              prepend: withCtx(() => [
                                createVNode(VIcon, {
                                  class: "me-2",
                                  icon: "tabler-user",
                                  size: "22"
                                })
                              ]),
                              default: withCtx(() => [
                                createVNode(VListItemTitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode("Profile")
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            }),
                            createVNode(VListItem, { link: "" }, {
                              prepend: withCtx(() => [
                                createVNode(VIcon, {
                                  class: "me-2",
                                  icon: "tabler-settings",
                                  size: "22"
                                })
                              ]),
                              default: withCtx(() => [
                                createVNode(VListItemTitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode("Settings")
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            }),
                            createVNode(VListItem, { link: "" }, {
                              prepend: withCtx(() => [
                                createVNode(VIcon, {
                                  class: "me-2",
                                  icon: "tabler-currency-dollar",
                                  size: "22"
                                })
                              ]),
                              default: withCtx(() => [
                                createVNode(VListItemTitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode("Pricing")
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            }),
                            createVNode(VListItem, { link: "" }, {
                              prepend: withCtx(() => [
                                createVNode(VIcon, {
                                  class: "me-2",
                                  icon: "tabler-help",
                                  size: "22"
                                })
                              ]),
                              default: withCtx(() => [
                                createVNode(VListItemTitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode("FAQ")
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            }),
                            createVNode(VDivider, { class: "my-2" }),
                            createVNode(VListItem, { to: "/login" }, {
                              prepend: withCtx(() => [
                                createVNode(VIcon, {
                                  class: "me-2",
                                  icon: "tabler-logout",
                                  size: "22"
                                })
                              ]),
                              default: withCtx(() => [
                                createVNode(VListItemTitle, null, {
                                  default: withCtx(() => [
                                    createTextVNode("Logout")
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
              createVNode(VAvatar, {
                class: "cursor-pointer",
                color: "primary",
                variant: "tonal"
              }, {
                default: withCtx(() => [
                  createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"]),
                  createVNode(VMenu, {
                    activator: "parent",
                    width: "230",
                    location: "bottom end",
                    offset: "14px"
                  }, {
                    default: withCtx(() => [
                      createVNode(VList, null, {
                        default: withCtx(() => [
                          createVNode(VListItem, null, {
                            prepend: withCtx(() => [
                              createVNode(VListItemAction, { start: "" }, {
                                default: withCtx(() => [
                                  createVNode(VBadge, {
                                    dot: "",
                                    location: "bottom right",
                                    "offset-x": "3",
                                    "offset-y": "3",
                                    color: "success"
                                  }, {
                                    default: withCtx(() => [
                                      createVNode(VAvatar, {
                                        color: "primary",
                                        variant: "tonal"
                                      }, {
                                        default: withCtx(() => [
                                          createVNode(VImg, { src: unref(avatar1) }, null, 8, ["src"])
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
                            default: withCtx(() => [
                              createVNode(VListItemTitle, { class: "font-weight-semibold" }, {
                                default: withCtx(() => [
                                  createTextVNode(" John Doe ")
                                ]),
                                _: 1
                              }),
                              createVNode(VListItemSubtitle, null, {
                                default: withCtx(() => [
                                  createTextVNode("Admin")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VDivider, { class: "my-2" }),
                          createVNode(VListItem, { link: "" }, {
                            prepend: withCtx(() => [
                              createVNode(VIcon, {
                                class: "me-2",
                                icon: "tabler-user",
                                size: "22"
                              })
                            ]),
                            default: withCtx(() => [
                              createVNode(VListItemTitle, null, {
                                default: withCtx(() => [
                                  createTextVNode("Profile")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VListItem, { link: "" }, {
                            prepend: withCtx(() => [
                              createVNode(VIcon, {
                                class: "me-2",
                                icon: "tabler-settings",
                                size: "22"
                              })
                            ]),
                            default: withCtx(() => [
                              createVNode(VListItemTitle, null, {
                                default: withCtx(() => [
                                  createTextVNode("Settings")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VListItem, { link: "" }, {
                            prepend: withCtx(() => [
                              createVNode(VIcon, {
                                class: "me-2",
                                icon: "tabler-currency-dollar",
                                size: "22"
                              })
                            ]),
                            default: withCtx(() => [
                              createVNode(VListItemTitle, null, {
                                default: withCtx(() => [
                                  createTextVNode("Pricing")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VListItem, { link: "" }, {
                            prepend: withCtx(() => [
                              createVNode(VIcon, {
                                class: "me-2",
                                icon: "tabler-help",
                                size: "22"
                              })
                            ]),
                            default: withCtx(() => [
                              createVNode(VListItemTitle, null, {
                                default: withCtx(() => [
                                  createTextVNode("FAQ")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VDivider, { class: "my-2" }),
                          createVNode(VListItem, { to: "/login" }, {
                            prepend: withCtx(() => [
                              createVNode(VIcon, {
                                class: "me-2",
                                icon: "tabler-logout",
                                size: "22"
                              })
                            ]),
                            default: withCtx(() => [
                              createVNode(VListItemTitle, null, {
                                default: withCtx(() => [
                                  createTextVNode("Logout")
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
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/components/UserProfile.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
