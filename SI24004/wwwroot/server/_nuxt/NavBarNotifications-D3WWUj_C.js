import { defineComponent, computed, resolveComponent, mergeProps, withCtx, createVNode, unref, createTextVNode, toDisplayString, withDirectives, vShow, openBlock, createBlock, createCommentVNode, withModifiers, Fragment, renderList, useSSRContext, ref } from "vue";
import { ssrRenderComponent, ssrInterpolate, ssrRenderList, ssrRenderStyle } from "vue/server-renderer";
import { PerfectScrollbar } from "vue3-perfect-scrollbar";
import { Y as VBadge, V as VIcon, U as VMenu, b as VCard, a9 as VCardItem, m as VChip, T as VTooltip, j as VCardTitle, s as VDivider, W as VList, X as VListItem, Z as VAvatar, S as VSpacer, a0 as VListItemTitle, d as VCardText, h as VBtn } from "../server.mjs";
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
const avatarText = (value) => {
  if (!value)
    return "";
  const nameArray = value.split(" ");
  return nameArray.map((word) => word.charAt(0).toUpperCase()).join("");
};
const _sfc_main$1 = /* @__PURE__ */ defineComponent({
  __name: "Notifications",
  __ssrInlineRender: true,
  props: {
    notifications: {},
    badgeProps: { default: void 0 },
    location: { default: "bottom end" }
  },
  emits: ["read", "unread", "remove", "click:notification"],
  setup(__props, { emit: __emit }) {
    const props = __props;
    const emit = __emit;
    const isAllMarkRead = computed(
      () => props.notifications.some((item) => item.isSeen === false)
    );
    const markAllReadOrUnread = () => {
      const allNotificationsIds = props.notifications.map((item) => item.id);
      if (!isAllMarkRead.value)
        emit("unread", allNotificationsIds);
      else
        emit("read", allNotificationsIds);
    };
    const totalUnseenNotifications = computed(() => {
      return props.notifications.filter((item) => item.isSeen === false).length;
    });
    const toggleReadUnread = (isSeen, Id) => {
      if (isSeen)
        emit("unread", [Id]);
      else
        emit("read", [Id]);
    };
    return (_ctx, _push, _parent, _attrs) => {
      const _component_IconBtn = resolveComponent("IconBtn");
      _push(ssrRenderComponent(_component_IconBtn, mergeProps({ id: "notification-btn" }, _attrs), {
        default: withCtx((_, _push2, _parent2, _scopeId) => {
          if (_push2) {
            _push2(ssrRenderComponent(VBadge, mergeProps(props.badgeProps, {
              "model-value": props.notifications.some((n) => !n.isSeen),
              color: "error",
              dot: "",
              "offset-x": "2",
              "offset-y": "3"
            }), {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VIcon, {
                    size: "24",
                    icon: "tabler-bell"
                  }, null, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VIcon, {
                      size: "24",
                      icon: "tabler-bell"
                    })
                  ];
                }
              }),
              _: 1
            }, _parent2, _scopeId));
            _push2(ssrRenderComponent(VMenu, {
              activator: "parent",
              width: "380px",
              location: props.location,
              offset: "12px",
              "close-on-content-click": false
            }, {
              default: withCtx((_2, _push3, _parent3, _scopeId2) => {
                if (_push3) {
                  _push3(ssrRenderComponent(VCard, { class: "d-flex flex-column" }, {
                    default: withCtx((_3, _push4, _parent4, _scopeId3) => {
                      if (_push4) {
                        _push4(ssrRenderComponent(VCardItem, { class: "notification-section" }, {
                          append: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VChip, {
                                style: props.notifications.some((n) => !n.isSeen) ? null : { display: "none" },
                                size: "small",
                                color: "primary",
                                class: "me-2"
                              }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(`${ssrInterpolate(unref(totalUnseenNotifications))} New `);
                                  } else {
                                    return [
                                      createTextVNode(toDisplayString(unref(totalUnseenNotifications)) + " New ", 1)
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                              _push5(ssrRenderComponent(_component_IconBtn, {
                                style: props.notifications.length ? null : { display: "none" },
                                size: "34",
                                onClick: markAllReadOrUnread
                              }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(ssrRenderComponent(VIcon, {
                                      size: "20",
                                      color: "high-emphasis",
                                      icon: !unref(isAllMarkRead) ? "tabler-mail" : "tabler-mail-opened"
                                    }, null, _parent6, _scopeId5));
                                    _push6(ssrRenderComponent(VTooltip, {
                                      activator: "parent",
                                      location: "start"
                                    }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(`${ssrInterpolate(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read")}`);
                                        } else {
                                          return [
                                            createTextVNode(toDisplayString(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read"), 1)
                                          ];
                                        }
                                      }),
                                      _: 1
                                    }, _parent6, _scopeId5));
                                  } else {
                                    return [
                                      createVNode(VIcon, {
                                        size: "20",
                                        color: "high-emphasis",
                                        icon: !unref(isAllMarkRead) ? "tabler-mail" : "tabler-mail-opened"
                                      }, null, 8, ["icon"]),
                                      createVNode(VTooltip, {
                                        activator: "parent",
                                        location: "start"
                                      }, {
                                        default: withCtx(() => [
                                          createTextVNode(toDisplayString(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read"), 1)
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
                                withDirectives(createVNode(VChip, {
                                  size: "small",
                                  color: "primary",
                                  class: "me-2"
                                }, {
                                  default: withCtx(() => [
                                    createTextVNode(toDisplayString(unref(totalUnseenNotifications)) + " New ", 1)
                                  ]),
                                  _: 1
                                }, 512), [
                                  [vShow, props.notifications.some((n) => !n.isSeen)]
                                ]),
                                withDirectives(createVNode(_component_IconBtn, {
                                  size: "34",
                                  onClick: markAllReadOrUnread
                                }, {
                                  default: withCtx(() => [
                                    createVNode(VIcon, {
                                      size: "20",
                                      color: "high-emphasis",
                                      icon: !unref(isAllMarkRead) ? "tabler-mail" : "tabler-mail-opened"
                                    }, null, 8, ["icon"]),
                                    createVNode(VTooltip, {
                                      activator: "parent",
                                      location: "start"
                                    }, {
                                      default: withCtx(() => [
                                        createTextVNode(toDisplayString(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read"), 1)
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }, 512), [
                                  [vShow, props.notifications.length]
                                ])
                              ];
                            }
                          }),
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VCardTitle, { class: "text-h6" }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(` Notifications `);
                                  } else {
                                    return [
                                      createTextVNode(" Notifications ")
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                            } else {
                              return [
                                createVNode(VCardTitle, { class: "text-h6" }, {
                                  default: withCtx(() => [
                                    createTextVNode(" Notifications ")
                                  ]),
                                  _: 1
                                })
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                        _push4(ssrRenderComponent(VDivider, null, null, _parent4, _scopeId3));
                        _push4(ssrRenderComponent(unref(PerfectScrollbar), {
                          options: { wheelPropagation: false },
                          style: { "max-block-size": "23.75rem" }
                        }, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VList, { class: "notification-list rounded-0 py-0" }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(`<!--[-->`);
                                    ssrRenderList(props.notifications, (notification, index) => {
                                      _push6(`<!--[-->`);
                                      if (index > 0) {
                                        _push6(ssrRenderComponent(VDivider, null, null, _parent6, _scopeId5));
                                      } else {
                                        _push6(`<!---->`);
                                      }
                                      _push6(ssrRenderComponent(VListItem, {
                                        link: "",
                                        lines: "one",
                                        "min-height": "66px",
                                        class: "list-item-hover-class",
                                        onClick: ($event) => _ctx.$emit("click:notification", notification)
                                      }, {
                                        default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                          if (_push7) {
                                            _push7(`<div class="d-flex align-start gap-3"${_scopeId6}>`);
                                            _push7(ssrRenderComponent(VAvatar, {
                                              size: "40",
                                              color: notification.color && notification.icon ? notification.color : void 0,
                                              image: notification.img || void 0,
                                              icon: notification.icon || void 0,
                                              variant: notification.img ? void 0 : "tonal"
                                            }, {
                                              default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                                if (_push8) {
                                                  if (notification.text) {
                                                    _push8(`<span${_scopeId7}>${ssrInterpolate(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text))}</span>`);
                                                  } else {
                                                    _push8(`<!---->`);
                                                  }
                                                } else {
                                                  return [
                                                    notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                                  ];
                                                }
                                              }),
                                              _: 2
                                            }, _parent7, _scopeId6));
                                            _push7(`<div${_scopeId6}><p class="text-sm font-weight-medium mb-1"${_scopeId6}>${ssrInterpolate(notification.title)}</p><p class="text-body-2 mb-2" style="${ssrRenderStyle({ "letter-spacing": "0.4px !important", "line-height": "18px" })}"${_scopeId6}>${ssrInterpolate(notification.subtitle)}</p><p class="text-sm text-disabled mb-0" style="${ssrRenderStyle({ "letter-spacing": "0.4px !important", "line-height": "18px" })}"${_scopeId6}>${ssrInterpolate(notification.time)}</p></div>`);
                                            _push7(ssrRenderComponent(VSpacer, null, null, _parent7, _scopeId6));
                                            _push7(`<div class="d-flex flex-column align-end"${_scopeId6}>`);
                                            _push7(ssrRenderComponent(VIcon, {
                                              size: "10",
                                              icon: "tabler-circle-filled",
                                              color: !notification.isSeen ? "primary" : "#a8aaae",
                                              class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                              onClick: ($event) => toggleReadUnread(notification.isSeen, notification.id)
                                            }, null, _parent7, _scopeId6));
                                            _push7(ssrRenderComponent(VIcon, {
                                              size: "20",
                                              icon: "tabler-x",
                                              class: "visible-in-hover",
                                              onClick: ($event) => _ctx.$emit("remove", notification.id)
                                            }, null, _parent7, _scopeId6));
                                            _push7(`</div></div>`);
                                          } else {
                                            return [
                                              createVNode("div", { class: "d-flex align-start gap-3" }, [
                                                createVNode(VAvatar, {
                                                  size: "40",
                                                  color: notification.color && notification.icon ? notification.color : void 0,
                                                  image: notification.img || void 0,
                                                  icon: notification.icon || void 0,
                                                  variant: notification.img ? void 0 : "tonal"
                                                }, {
                                                  default: withCtx(() => [
                                                    notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                                  ]),
                                                  _: 2
                                                }, 1032, ["color", "image", "icon", "variant"]),
                                                createVNode("div", null, [
                                                  createVNode("p", { class: "text-sm font-weight-medium mb-1" }, toDisplayString(notification.title), 1),
                                                  createVNode("p", {
                                                    class: "text-body-2 mb-2",
                                                    style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                                  }, toDisplayString(notification.subtitle), 1),
                                                  createVNode("p", {
                                                    class: "text-sm text-disabled mb-0",
                                                    style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                                  }, toDisplayString(notification.time), 1)
                                                ]),
                                                createVNode(VSpacer),
                                                createVNode("div", { class: "d-flex flex-column align-end" }, [
                                                  createVNode(VIcon, {
                                                    size: "10",
                                                    icon: "tabler-circle-filled",
                                                    color: !notification.isSeen ? "primary" : "#a8aaae",
                                                    class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                                    onClick: withModifiers(($event) => toggleReadUnread(notification.isSeen, notification.id), ["stop"])
                                                  }, null, 8, ["color", "class", "onClick"]),
                                                  createVNode(VIcon, {
                                                    size: "20",
                                                    icon: "tabler-x",
                                                    class: "visible-in-hover",
                                                    onClick: ($event) => _ctx.$emit("remove", notification.id)
                                                  }, null, 8, ["onClick"])
                                                ])
                                              ])
                                            ];
                                          }
                                        }),
                                        _: 2
                                      }, _parent6, _scopeId5));
                                      _push6(`<!--]-->`);
                                    });
                                    _push6(`<!--]-->`);
                                    _push6(ssrRenderComponent(VListItem, {
                                      style: [
                                        !props.notifications.length ? null : { display: "none" },
                                        { "block-size": "56px" }
                                      ],
                                      class: "text-center text-medium-emphasis"
                                    }, {
                                      default: withCtx((_6, _push7, _parent7, _scopeId6) => {
                                        if (_push7) {
                                          _push7(ssrRenderComponent(VListItemTitle, null, {
                                            default: withCtx((_7, _push8, _parent8, _scopeId7) => {
                                              if (_push8) {
                                                _push8(`No Notification Found!`);
                                              } else {
                                                return [
                                                  createTextVNode("No Notification Found!")
                                                ];
                                              }
                                            }),
                                            _: 1
                                          }, _parent7, _scopeId6));
                                        } else {
                                          return [
                                            createVNode(VListItemTitle, null, {
                                              default: withCtx(() => [
                                                createTextVNode("No Notification Found!")
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
                                      (openBlock(true), createBlock(Fragment, null, renderList(props.notifications, (notification, index) => {
                                        return openBlock(), createBlock(Fragment, {
                                          key: notification.title
                                        }, [
                                          index > 0 ? (openBlock(), createBlock(VDivider, { key: 0 })) : createCommentVNode("", true),
                                          createVNode(VListItem, {
                                            link: "",
                                            lines: "one",
                                            "min-height": "66px",
                                            class: "list-item-hover-class",
                                            onClick: ($event) => _ctx.$emit("click:notification", notification)
                                          }, {
                                            default: withCtx(() => [
                                              createVNode("div", { class: "d-flex align-start gap-3" }, [
                                                createVNode(VAvatar, {
                                                  size: "40",
                                                  color: notification.color && notification.icon ? notification.color : void 0,
                                                  image: notification.img || void 0,
                                                  icon: notification.icon || void 0,
                                                  variant: notification.img ? void 0 : "tonal"
                                                }, {
                                                  default: withCtx(() => [
                                                    notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                                  ]),
                                                  _: 2
                                                }, 1032, ["color", "image", "icon", "variant"]),
                                                createVNode("div", null, [
                                                  createVNode("p", { class: "text-sm font-weight-medium mb-1" }, toDisplayString(notification.title), 1),
                                                  createVNode("p", {
                                                    class: "text-body-2 mb-2",
                                                    style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                                  }, toDisplayString(notification.subtitle), 1),
                                                  createVNode("p", {
                                                    class: "text-sm text-disabled mb-0",
                                                    style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                                  }, toDisplayString(notification.time), 1)
                                                ]),
                                                createVNode(VSpacer),
                                                createVNode("div", { class: "d-flex flex-column align-end" }, [
                                                  createVNode(VIcon, {
                                                    size: "10",
                                                    icon: "tabler-circle-filled",
                                                    color: !notification.isSeen ? "primary" : "#a8aaae",
                                                    class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                                    onClick: withModifiers(($event) => toggleReadUnread(notification.isSeen, notification.id), ["stop"])
                                                  }, null, 8, ["color", "class", "onClick"]),
                                                  createVNode(VIcon, {
                                                    size: "20",
                                                    icon: "tabler-x",
                                                    class: "visible-in-hover",
                                                    onClick: ($event) => _ctx.$emit("remove", notification.id)
                                                  }, null, 8, ["onClick"])
                                                ])
                                              ])
                                            ]),
                                            _: 2
                                          }, 1032, ["onClick"])
                                        ], 64);
                                      }), 128)),
                                      withDirectives(createVNode(VListItem, {
                                        class: "text-center text-medium-emphasis",
                                        style: { "block-size": "56px" }
                                      }, {
                                        default: withCtx(() => [
                                          createVNode(VListItemTitle, null, {
                                            default: withCtx(() => [
                                              createTextVNode("No Notification Found!")
                                            ]),
                                            _: 1
                                          })
                                        ]),
                                        _: 1
                                      }, 512), [
                                        [vShow, !props.notifications.length]
                                      ])
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                            } else {
                              return [
                                createVNode(VList, { class: "notification-list rounded-0 py-0" }, {
                                  default: withCtx(() => [
                                    (openBlock(true), createBlock(Fragment, null, renderList(props.notifications, (notification, index) => {
                                      return openBlock(), createBlock(Fragment, {
                                        key: notification.title
                                      }, [
                                        index > 0 ? (openBlock(), createBlock(VDivider, { key: 0 })) : createCommentVNode("", true),
                                        createVNode(VListItem, {
                                          link: "",
                                          lines: "one",
                                          "min-height": "66px",
                                          class: "list-item-hover-class",
                                          onClick: ($event) => _ctx.$emit("click:notification", notification)
                                        }, {
                                          default: withCtx(() => [
                                            createVNode("div", { class: "d-flex align-start gap-3" }, [
                                              createVNode(VAvatar, {
                                                size: "40",
                                                color: notification.color && notification.icon ? notification.color : void 0,
                                                image: notification.img || void 0,
                                                icon: notification.icon || void 0,
                                                variant: notification.img ? void 0 : "tonal"
                                              }, {
                                                default: withCtx(() => [
                                                  notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                                ]),
                                                _: 2
                                              }, 1032, ["color", "image", "icon", "variant"]),
                                              createVNode("div", null, [
                                                createVNode("p", { class: "text-sm font-weight-medium mb-1" }, toDisplayString(notification.title), 1),
                                                createVNode("p", {
                                                  class: "text-body-2 mb-2",
                                                  style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                                }, toDisplayString(notification.subtitle), 1),
                                                createVNode("p", {
                                                  class: "text-sm text-disabled mb-0",
                                                  style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                                }, toDisplayString(notification.time), 1)
                                              ]),
                                              createVNode(VSpacer),
                                              createVNode("div", { class: "d-flex flex-column align-end" }, [
                                                createVNode(VIcon, {
                                                  size: "10",
                                                  icon: "tabler-circle-filled",
                                                  color: !notification.isSeen ? "primary" : "#a8aaae",
                                                  class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                                  onClick: withModifiers(($event) => toggleReadUnread(notification.isSeen, notification.id), ["stop"])
                                                }, null, 8, ["color", "class", "onClick"]),
                                                createVNode(VIcon, {
                                                  size: "20",
                                                  icon: "tabler-x",
                                                  class: "visible-in-hover",
                                                  onClick: ($event) => _ctx.$emit("remove", notification.id)
                                                }, null, 8, ["onClick"])
                                              ])
                                            ])
                                          ]),
                                          _: 2
                                        }, 1032, ["onClick"])
                                      ], 64);
                                    }), 128)),
                                    withDirectives(createVNode(VListItem, {
                                      class: "text-center text-medium-emphasis",
                                      style: { "block-size": "56px" }
                                    }, {
                                      default: withCtx(() => [
                                        createVNode(VListItemTitle, null, {
                                          default: withCtx(() => [
                                            createTextVNode("No Notification Found!")
                                          ]),
                                          _: 1
                                        })
                                      ]),
                                      _: 1
                                    }, 512), [
                                      [vShow, !props.notifications.length]
                                    ])
                                  ]),
                                  _: 1
                                })
                              ];
                            }
                          }),
                          _: 1
                        }, _parent4, _scopeId3));
                        _push4(ssrRenderComponent(VDivider, null, null, _parent4, _scopeId3));
                        _push4(ssrRenderComponent(VCardText, {
                          style: props.notifications.length ? null : { display: "none" },
                          class: "pa-4"
                        }, {
                          default: withCtx((_4, _push5, _parent5, _scopeId4) => {
                            if (_push5) {
                              _push5(ssrRenderComponent(VBtn, {
                                block: "",
                                size: "small"
                              }, {
                                default: withCtx((_5, _push6, _parent6, _scopeId5) => {
                                  if (_push6) {
                                    _push6(` View All Notifications `);
                                  } else {
                                    return [
                                      createTextVNode(" View All Notifications ")
                                    ];
                                  }
                                }),
                                _: 1
                              }, _parent5, _scopeId4));
                            } else {
                              return [
                                createVNode(VBtn, {
                                  block: "",
                                  size: "small"
                                }, {
                                  default: withCtx(() => [
                                    createTextVNode(" View All Notifications ")
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
                          createVNode(VCardItem, { class: "notification-section" }, {
                            append: withCtx(() => [
                              withDirectives(createVNode(VChip, {
                                size: "small",
                                color: "primary",
                                class: "me-2"
                              }, {
                                default: withCtx(() => [
                                  createTextVNode(toDisplayString(unref(totalUnseenNotifications)) + " New ", 1)
                                ]),
                                _: 1
                              }, 512), [
                                [vShow, props.notifications.some((n) => !n.isSeen)]
                              ]),
                              withDirectives(createVNode(_component_IconBtn, {
                                size: "34",
                                onClick: markAllReadOrUnread
                              }, {
                                default: withCtx(() => [
                                  createVNode(VIcon, {
                                    size: "20",
                                    color: "high-emphasis",
                                    icon: !unref(isAllMarkRead) ? "tabler-mail" : "tabler-mail-opened"
                                  }, null, 8, ["icon"]),
                                  createVNode(VTooltip, {
                                    activator: "parent",
                                    location: "start"
                                  }, {
                                    default: withCtx(() => [
                                      createTextVNode(toDisplayString(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read"), 1)
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }, 512), [
                                [vShow, props.notifications.length]
                              ])
                            ]),
                            default: withCtx(() => [
                              createVNode(VCardTitle, { class: "text-h6" }, {
                                default: withCtx(() => [
                                  createTextVNode(" Notifications ")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VDivider),
                          createVNode(unref(PerfectScrollbar), {
                            options: { wheelPropagation: false },
                            style: { "max-block-size": "23.75rem" }
                          }, {
                            default: withCtx(() => [
                              createVNode(VList, { class: "notification-list rounded-0 py-0" }, {
                                default: withCtx(() => [
                                  (openBlock(true), createBlock(Fragment, null, renderList(props.notifications, (notification, index) => {
                                    return openBlock(), createBlock(Fragment, {
                                      key: notification.title
                                    }, [
                                      index > 0 ? (openBlock(), createBlock(VDivider, { key: 0 })) : createCommentVNode("", true),
                                      createVNode(VListItem, {
                                        link: "",
                                        lines: "one",
                                        "min-height": "66px",
                                        class: "list-item-hover-class",
                                        onClick: ($event) => _ctx.$emit("click:notification", notification)
                                      }, {
                                        default: withCtx(() => [
                                          createVNode("div", { class: "d-flex align-start gap-3" }, [
                                            createVNode(VAvatar, {
                                              size: "40",
                                              color: notification.color && notification.icon ? notification.color : void 0,
                                              image: notification.img || void 0,
                                              icon: notification.icon || void 0,
                                              variant: notification.img ? void 0 : "tonal"
                                            }, {
                                              default: withCtx(() => [
                                                notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                              ]),
                                              _: 2
                                            }, 1032, ["color", "image", "icon", "variant"]),
                                            createVNode("div", null, [
                                              createVNode("p", { class: "text-sm font-weight-medium mb-1" }, toDisplayString(notification.title), 1),
                                              createVNode("p", {
                                                class: "text-body-2 mb-2",
                                                style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                              }, toDisplayString(notification.subtitle), 1),
                                              createVNode("p", {
                                                class: "text-sm text-disabled mb-0",
                                                style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                              }, toDisplayString(notification.time), 1)
                                            ]),
                                            createVNode(VSpacer),
                                            createVNode("div", { class: "d-flex flex-column align-end" }, [
                                              createVNode(VIcon, {
                                                size: "10",
                                                icon: "tabler-circle-filled",
                                                color: !notification.isSeen ? "primary" : "#a8aaae",
                                                class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                                onClick: withModifiers(($event) => toggleReadUnread(notification.isSeen, notification.id), ["stop"])
                                              }, null, 8, ["color", "class", "onClick"]),
                                              createVNode(VIcon, {
                                                size: "20",
                                                icon: "tabler-x",
                                                class: "visible-in-hover",
                                                onClick: ($event) => _ctx.$emit("remove", notification.id)
                                              }, null, 8, ["onClick"])
                                            ])
                                          ])
                                        ]),
                                        _: 2
                                      }, 1032, ["onClick"])
                                    ], 64);
                                  }), 128)),
                                  withDirectives(createVNode(VListItem, {
                                    class: "text-center text-medium-emphasis",
                                    style: { "block-size": "56px" }
                                  }, {
                                    default: withCtx(() => [
                                      createVNode(VListItemTitle, null, {
                                        default: withCtx(() => [
                                          createTextVNode("No Notification Found!")
                                        ]),
                                        _: 1
                                      })
                                    ]),
                                    _: 1
                                  }, 512), [
                                    [vShow, !props.notifications.length]
                                  ])
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }),
                          createVNode(VDivider),
                          withDirectives(createVNode(VCardText, { class: "pa-4" }, {
                            default: withCtx(() => [
                              createVNode(VBtn, {
                                block: "",
                                size: "small"
                              }, {
                                default: withCtx(() => [
                                  createTextVNode(" View All Notifications ")
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }, 512), [
                            [vShow, props.notifications.length]
                          ])
                        ];
                      }
                    }),
                    _: 1
                  }, _parent3, _scopeId2));
                } else {
                  return [
                    createVNode(VCard, { class: "d-flex flex-column" }, {
                      default: withCtx(() => [
                        createVNode(VCardItem, { class: "notification-section" }, {
                          append: withCtx(() => [
                            withDirectives(createVNode(VChip, {
                              size: "small",
                              color: "primary",
                              class: "me-2"
                            }, {
                              default: withCtx(() => [
                                createTextVNode(toDisplayString(unref(totalUnseenNotifications)) + " New ", 1)
                              ]),
                              _: 1
                            }, 512), [
                              [vShow, props.notifications.some((n) => !n.isSeen)]
                            ]),
                            withDirectives(createVNode(_component_IconBtn, {
                              size: "34",
                              onClick: markAllReadOrUnread
                            }, {
                              default: withCtx(() => [
                                createVNode(VIcon, {
                                  size: "20",
                                  color: "high-emphasis",
                                  icon: !unref(isAllMarkRead) ? "tabler-mail" : "tabler-mail-opened"
                                }, null, 8, ["icon"]),
                                createVNode(VTooltip, {
                                  activator: "parent",
                                  location: "start"
                                }, {
                                  default: withCtx(() => [
                                    createTextVNode(toDisplayString(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read"), 1)
                                  ]),
                                  _: 1
                                })
                              ]),
                              _: 1
                            }, 512), [
                              [vShow, props.notifications.length]
                            ])
                          ]),
                          default: withCtx(() => [
                            createVNode(VCardTitle, { class: "text-h6" }, {
                              default: withCtx(() => [
                                createTextVNode(" Notifications ")
                              ]),
                              _: 1
                            })
                          ]),
                          _: 1
                        }),
                        createVNode(VDivider),
                        createVNode(unref(PerfectScrollbar), {
                          options: { wheelPropagation: false },
                          style: { "max-block-size": "23.75rem" }
                        }, {
                          default: withCtx(() => [
                            createVNode(VList, { class: "notification-list rounded-0 py-0" }, {
                              default: withCtx(() => [
                                (openBlock(true), createBlock(Fragment, null, renderList(props.notifications, (notification, index) => {
                                  return openBlock(), createBlock(Fragment, {
                                    key: notification.title
                                  }, [
                                    index > 0 ? (openBlock(), createBlock(VDivider, { key: 0 })) : createCommentVNode("", true),
                                    createVNode(VListItem, {
                                      link: "",
                                      lines: "one",
                                      "min-height": "66px",
                                      class: "list-item-hover-class",
                                      onClick: ($event) => _ctx.$emit("click:notification", notification)
                                    }, {
                                      default: withCtx(() => [
                                        createVNode("div", { class: "d-flex align-start gap-3" }, [
                                          createVNode(VAvatar, {
                                            size: "40",
                                            color: notification.color && notification.icon ? notification.color : void 0,
                                            image: notification.img || void 0,
                                            icon: notification.icon || void 0,
                                            variant: notification.img ? void 0 : "tonal"
                                          }, {
                                            default: withCtx(() => [
                                              notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                            ]),
                                            _: 2
                                          }, 1032, ["color", "image", "icon", "variant"]),
                                          createVNode("div", null, [
                                            createVNode("p", { class: "text-sm font-weight-medium mb-1" }, toDisplayString(notification.title), 1),
                                            createVNode("p", {
                                              class: "text-body-2 mb-2",
                                              style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                            }, toDisplayString(notification.subtitle), 1),
                                            createVNode("p", {
                                              class: "text-sm text-disabled mb-0",
                                              style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                            }, toDisplayString(notification.time), 1)
                                          ]),
                                          createVNode(VSpacer),
                                          createVNode("div", { class: "d-flex flex-column align-end" }, [
                                            createVNode(VIcon, {
                                              size: "10",
                                              icon: "tabler-circle-filled",
                                              color: !notification.isSeen ? "primary" : "#a8aaae",
                                              class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                              onClick: withModifiers(($event) => toggleReadUnread(notification.isSeen, notification.id), ["stop"])
                                            }, null, 8, ["color", "class", "onClick"]),
                                            createVNode(VIcon, {
                                              size: "20",
                                              icon: "tabler-x",
                                              class: "visible-in-hover",
                                              onClick: ($event) => _ctx.$emit("remove", notification.id)
                                            }, null, 8, ["onClick"])
                                          ])
                                        ])
                                      ]),
                                      _: 2
                                    }, 1032, ["onClick"])
                                  ], 64);
                                }), 128)),
                                withDirectives(createVNode(VListItem, {
                                  class: "text-center text-medium-emphasis",
                                  style: { "block-size": "56px" }
                                }, {
                                  default: withCtx(() => [
                                    createVNode(VListItemTitle, null, {
                                      default: withCtx(() => [
                                        createTextVNode("No Notification Found!")
                                      ]),
                                      _: 1
                                    })
                                  ]),
                                  _: 1
                                }, 512), [
                                  [vShow, !props.notifications.length]
                                ])
                              ]),
                              _: 1
                            })
                          ]),
                          _: 1
                        }),
                        createVNode(VDivider),
                        withDirectives(createVNode(VCardText, { class: "pa-4" }, {
                          default: withCtx(() => [
                            createVNode(VBtn, {
                              block: "",
                              size: "small"
                            }, {
                              default: withCtx(() => [
                                createTextVNode(" View All Notifications ")
                              ]),
                              _: 1
                            })
                          ]),
                          _: 1
                        }, 512), [
                          [vShow, props.notifications.length]
                        ])
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
              createVNode(VBadge, mergeProps(props.badgeProps, {
                "model-value": props.notifications.some((n) => !n.isSeen),
                color: "error",
                dot: "",
                "offset-x": "2",
                "offset-y": "3"
              }), {
                default: withCtx(() => [
                  createVNode(VIcon, {
                    size: "24",
                    icon: "tabler-bell"
                  })
                ]),
                _: 1
              }, 16, ["model-value"]),
              createVNode(VMenu, {
                activator: "parent",
                width: "380px",
                location: props.location,
                offset: "12px",
                "close-on-content-click": false
              }, {
                default: withCtx(() => [
                  createVNode(VCard, { class: "d-flex flex-column" }, {
                    default: withCtx(() => [
                      createVNode(VCardItem, { class: "notification-section" }, {
                        append: withCtx(() => [
                          withDirectives(createVNode(VChip, {
                            size: "small",
                            color: "primary",
                            class: "me-2"
                          }, {
                            default: withCtx(() => [
                              createTextVNode(toDisplayString(unref(totalUnseenNotifications)) + " New ", 1)
                            ]),
                            _: 1
                          }, 512), [
                            [vShow, props.notifications.some((n) => !n.isSeen)]
                          ]),
                          withDirectives(createVNode(_component_IconBtn, {
                            size: "34",
                            onClick: markAllReadOrUnread
                          }, {
                            default: withCtx(() => [
                              createVNode(VIcon, {
                                size: "20",
                                color: "high-emphasis",
                                icon: !unref(isAllMarkRead) ? "tabler-mail" : "tabler-mail-opened"
                              }, null, 8, ["icon"]),
                              createVNode(VTooltip, {
                                activator: "parent",
                                location: "start"
                              }, {
                                default: withCtx(() => [
                                  createTextVNode(toDisplayString(!unref(isAllMarkRead) ? "Mark all as unread" : "Mark all as read"), 1)
                                ]),
                                _: 1
                              })
                            ]),
                            _: 1
                          }, 512), [
                            [vShow, props.notifications.length]
                          ])
                        ]),
                        default: withCtx(() => [
                          createVNode(VCardTitle, { class: "text-h6" }, {
                            default: withCtx(() => [
                              createTextVNode(" Notifications ")
                            ]),
                            _: 1
                          })
                        ]),
                        _: 1
                      }),
                      createVNode(VDivider),
                      createVNode(unref(PerfectScrollbar), {
                        options: { wheelPropagation: false },
                        style: { "max-block-size": "23.75rem" }
                      }, {
                        default: withCtx(() => [
                          createVNode(VList, { class: "notification-list rounded-0 py-0" }, {
                            default: withCtx(() => [
                              (openBlock(true), createBlock(Fragment, null, renderList(props.notifications, (notification, index) => {
                                return openBlock(), createBlock(Fragment, {
                                  key: notification.title
                                }, [
                                  index > 0 ? (openBlock(), createBlock(VDivider, { key: 0 })) : createCommentVNode("", true),
                                  createVNode(VListItem, {
                                    link: "",
                                    lines: "one",
                                    "min-height": "66px",
                                    class: "list-item-hover-class",
                                    onClick: ($event) => _ctx.$emit("click:notification", notification)
                                  }, {
                                    default: withCtx(() => [
                                      createVNode("div", { class: "d-flex align-start gap-3" }, [
                                        createVNode(VAvatar, {
                                          size: "40",
                                          color: notification.color && notification.icon ? notification.color : void 0,
                                          image: notification.img || void 0,
                                          icon: notification.icon || void 0,
                                          variant: notification.img ? void 0 : "tonal"
                                        }, {
                                          default: withCtx(() => [
                                            notification.text ? (openBlock(), createBlock("span", { key: 0 }, toDisplayString(("avatarText" in _ctx ? _ctx.avatarText : unref(avatarText))(notification.text)), 1)) : createCommentVNode("", true)
                                          ]),
                                          _: 2
                                        }, 1032, ["color", "image", "icon", "variant"]),
                                        createVNode("div", null, [
                                          createVNode("p", { class: "text-sm font-weight-medium mb-1" }, toDisplayString(notification.title), 1),
                                          createVNode("p", {
                                            class: "text-body-2 mb-2",
                                            style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                          }, toDisplayString(notification.subtitle), 1),
                                          createVNode("p", {
                                            class: "text-sm text-disabled mb-0",
                                            style: { "letter-spacing": "0.4px !important", "line-height": "18px" }
                                          }, toDisplayString(notification.time), 1)
                                        ]),
                                        createVNode(VSpacer),
                                        createVNode("div", { class: "d-flex flex-column align-end" }, [
                                          createVNode(VIcon, {
                                            size: "10",
                                            icon: "tabler-circle-filled",
                                            color: !notification.isSeen ? "primary" : "#a8aaae",
                                            class: [`${notification.isSeen ? "visible-in-hover" : ""}`, "mb-2"],
                                            onClick: withModifiers(($event) => toggleReadUnread(notification.isSeen, notification.id), ["stop"])
                                          }, null, 8, ["color", "class", "onClick"]),
                                          createVNode(VIcon, {
                                            size: "20",
                                            icon: "tabler-x",
                                            class: "visible-in-hover",
                                            onClick: ($event) => _ctx.$emit("remove", notification.id)
                                          }, null, 8, ["onClick"])
                                        ])
                                      ])
                                    ]),
                                    _: 2
                                  }, 1032, ["onClick"])
                                ], 64);
                              }), 128)),
                              withDirectives(createVNode(VListItem, {
                                class: "text-center text-medium-emphasis",
                                style: { "block-size": "56px" }
                              }, {
                                default: withCtx(() => [
                                  createVNode(VListItemTitle, null, {
                                    default: withCtx(() => [
                                      createTextVNode("No Notification Found!")
                                    ]),
                                    _: 1
                                  })
                                ]),
                                _: 1
                              }, 512), [
                                [vShow, !props.notifications.length]
                              ])
                            ]),
                            _: 1
                          })
                        ]),
                        _: 1
                      }),
                      createVNode(VDivider),
                      withDirectives(createVNode(VCardText, { class: "pa-4" }, {
                        default: withCtx(() => [
                          createVNode(VBtn, {
                            block: "",
                            size: "small"
                          }, {
                            default: withCtx(() => [
                              createTextVNode(" View All Notifications ")
                            ]),
                            _: 1
                          })
                        ]),
                        _: 1
                      }, 512), [
                        [vShow, props.notifications.length]
                      ])
                    ]),
                    _: 1
                  })
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
const _sfc_setup$1 = _sfc_main$1.setup;
_sfc_main$1.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("@core/components/Notifications.vue");
  return _sfc_setup$1 ? _sfc_setup$1(props, ctx) : void 0;
};
const avatar3 = "" + __buildAssetsURL("avatar-3.BxDW4ia1.png");
const avatar4 = "" + __buildAssetsURL("avatar-4.CtU30128.png");
const avatar5 = "" + __buildAssetsURL("avatar-5.CmycerLe.png");
const paypal = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACYAAAAmCAYAAACoPemuAAAAAXNSR0IArs4c6QAAA1ZJREFUWEfNmM1PE0EYxp/pbrcUsLUqARMSjUf/Ak+Gg4kBY0jQTfDGiTN/CmdOHgmLB0wUYzx4gj35ceHgVwwpUVCElo/a7W7HzLTbbpfuzttWSPe6M+/89nnf95nZYejw4QDDxFwK+kkKaT0Jp5xEAhoYNBmKw0MVHoxUBSW3AneojLdPy0y86eBh1LF8fj6JbweDMJAGvBR1Xm2cVoaDEm7lTtnSUoUyVwnGTVPDAYYxqA3BdZKUoJFjdKOCU+8EORwzy/LiYsWCcdNMo+Begs7SPQGFJ7u8hKx+xCyrFBU3Eoyb5jBKyKLq6f8Vyg+W0FykUWCWddwuflswPjmZgWdkoWuJc4Hyg7peFZpTYOvrxfA6Z8CkUgUnd+5QQbiscRBWrgVM1lQJV84tfVHy19L6J1hzDTDZfQX3qrLQJ+7ewUhuAZp+JzLNnmvDcW35fu3FIqkcag2x73drE+yemYXhXVYGmX6wgPTAgnJccEDp7yIJ0NEO2RurIKZKMGmeP4sjJJ+affS9Iyh/8PKzm8p5wufGMr+ECdfAqGqJNI6NLCsXaDegQ9WY3PumzFHSNtNNGn1IUXfW2qz6o7QyXlq7jE/MDWCwOKqeAOBCwACcZnYZOY2CvBew/M4WNu3X8KDuUtEE/OGTa/DKQyTFzOnlWJuIC7L+Ko+j43FUmboJtNQJ4/dnrkODQQLrtiNF8JXV2hKMLSpV8+AwPjkz3jjkqei6BdvYtJHfqRsym0UVNfONejg8xqdmbqh45PturULU1sbm7eYaBDAhLBmsm8J/9z6PL1/HWz6cUmMSjJpKKphQaG8vg9/7Ng4PH4eyYaPK1F4mU0kt/qiOPJOquMKgpRGy+Kl2EVX4nz6v4sPHsDJn6Sjd6M+SdkHdJ6PAfH+KEkoAcdjKTgzOlwZL2ZLiOnLt+RbKTqDrYIMxu2OYIJjckiibeFzhr6wKT2oeGjtJWVuV65u4eKdMZ5xivqM3FiEWeFTq64dF+kFRqGYEjtM/djPY3i42Hd1fqQew8EGRpFrwCxMidbz9gZFooG0FCx+tJRj1Z8SPmJBgrT8kvdRX1M9IHa7/ft98Ifryh7cB149XBC3K9dulSgCu/66hAnD9d3EX9Jq+u+oMG+FFXQ7/A7YTzYZ/kuhAAAAAAElFTkSuQmCC";
const _sfc_main = /* @__PURE__ */ defineComponent({
  __name: "NavBarNotifications",
  __ssrInlineRender: true,
  setup(__props) {
    const notifications = ref([
      {
        id: 1,
        img: avatar4,
        title: "Congratulation Flora! 🎉",
        subtitle: "Won the monthly best seller badge",
        time: "Today",
        isSeen: true
      },
      {
        id: 2,
        text: "Tom Holland",
        title: "New user registered.",
        subtitle: "5 hours ago",
        time: "Yesterday",
        isSeen: false
      },
      {
        id: 3,
        img: avatar5,
        title: "New message received 👋🏻",
        subtitle: "You have 10 unread messages",
        time: "11 Aug",
        isSeen: true
      },
      {
        id: 4,
        img: paypal,
        title: "PayPal",
        subtitle: "Received Payment",
        time: "25 May",
        isSeen: false,
        color: "error"
      },
      {
        id: 5,
        img: avatar3,
        title: "Received Order 📦",
        subtitle: "New order received from john",
        time: "19 Mar",
        isSeen: true
      }
    ]);
    const removeNotification = (notificationId) => {
      notifications.value.forEach((item, index) => {
        if (notificationId === item.id)
          notifications.value.splice(index, 1);
      });
    };
    const markRead = (notificationId) => {
      notifications.value.forEach((item) => {
        notificationId.forEach((id) => {
          if (id === item.id)
            item.isSeen = true;
        });
      });
    };
    const markUnRead = (notificationId) => {
      notifications.value.forEach((item) => {
        notificationId.forEach((id) => {
          if (id === item.id)
            item.isSeen = false;
        });
      });
    };
    const handleNotificationClick = (notification) => {
      if (!notification.isSeen)
        markRead([notification.id]);
    };
    return (_ctx, _push, _parent, _attrs) => {
      const _component_Notifications = _sfc_main$1;
      _push(ssrRenderComponent(_component_Notifications, mergeProps({
        notifications: unref(notifications),
        onRemove: removeNotification,
        onRead: markRead,
        onUnread: markUnRead,
        "onClick:notification": handleNotificationClick
      }, _attrs), null, _parent));
    };
  }
});
const _sfc_setup = _sfc_main.setup;
_sfc_main.setup = (props, ctx) => {
  const ssrContext = useSSRContext();
  (ssrContext.modules || (ssrContext.modules = /* @__PURE__ */ new Set())).add("layouts/components/NavBarNotifications.vue");
  return _sfc_setup ? _sfc_setup(props, ctx) : void 0;
};
export {
  _sfc_main as default
};
