import{ai as l}from"./entry.ClxI4Ktn.js";/**
 * @license lucide-vue-next v0.546.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const u=e=>e.replace(/([a-z0-9])([A-Z])/g,"$1-$2").toLowerCase(),y=e=>e.replace(/^([A-Z])|[\s-_]+(\w)/g,(t,c,r)=>r?r.toUpperCase():c.toLowerCase()),C=e=>{const t=y(e);return t.charAt(0).toUpperCase()+t.slice(1)},g=(...e)=>e.filter((t,c,r)=>!!t&&t.trim()!==""&&r.indexOf(t)===c).join(" ").trim(),h=e=>e==="";/**
 * @license lucide-vue-next v0.546.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */var o={xmlns:"http://www.w3.org/2000/svg",width:24,height:24,viewBox:"0 0 24 24",fill:"none",stroke:"currentColor","stroke-width":2,"stroke-linecap":"round","stroke-linejoin":"round"};/**
 * @license lucide-vue-next v0.546.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const m=({name:e,iconNode:t,absoluteStrokeWidth:c,"absolute-stroke-width":r,strokeWidth:i,"stroke-width":n,size:s=o.width,color:k=o.stroke,...a},{slots:d})=>l("svg",{...o,...a,width:s,height:s,stroke:k,"stroke-width":h(c)||h(r)||c===!0||r===!0?Number(i||n||o["stroke-width"])*24/Number(s):i||n||o["stroke-width"],class:g("lucide",a.class,...e?[`lucide-${u(C(e))}-icon`,`lucide-${u(e)}`]:["lucide-icon"])},[...t.map(p=>l(...p)),...d.default?[d.default()]:[]]);/**
 * @license lucide-vue-next v0.546.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const w=(e,t)=>(c,{slots:r,attrs:i})=>l(m,{...i,...c,iconNode:t,name:e},r);/**
 * @license lucide-vue-next v0.546.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const x=w("circle-alert",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["line",{x1:"12",x2:"12",y1:"8",y2:"12",key:"1pkeuh"}],["line",{x1:"12",x2:"12.01",y1:"16",y2:"16",key:"4dfq90"}]]);/**
 * @license lucide-vue-next v0.546.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const A=w("circle-check-big",[["path",{d:"M21.801 10A10 10 0 1 1 17 3.335",key:"yps3ct"}],["path",{d:"m9 11 3 3L22 4",key:"1pflzl"}]]);export{x as C,A as a,w as c};
