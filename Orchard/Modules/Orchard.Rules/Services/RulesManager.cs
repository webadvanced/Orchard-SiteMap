using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Rules.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Scripting;
using Orchard.Tokens;

namespace Orchard.Rules.Services {
    public class RulesManager : IRulesManager {
        private readonly IRepository<EventRecord> _eventRepository;
        private readonly IRepository<RuleRecord> _ruleRepository;
        private readonly IEnumerable<IEventProvider> _eventProviders;
        private readonly IEnumerable<IActionProvider> _actionProviders;
        private readonly ITokenizer _tokenizer;

        public RulesManager(
            IRepository<EventRecord> eventRepository,
            IRepository<RuleRecord> ruleRepository,
            IEnumerable<IEventProvider> eventProviders,
            IEnumerable<IActionProvider> actionProviders,
            ITokenizer tokenizer) {
            _eventRepository = eventRepository;
            _ruleRepository = ruleRepository;
            _eventProviders = eventProviders;
            _actionProviders = actionProviders;
            _tokenizer = tokenizer;
        }

        public Localizer T { get; set; }

        public IEnumerable<TypeDescriptor<EventDescriptor>> DescribeEvents() {
            var context = new DescribeEventContext();
            foreach (var provider in _eventProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<ActionDescriptor>> DescribeActions() {
            var context = new DescribeActionContext();
            foreach (var provider in _actionProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public void TriggerEvent(string category, string type, Func<Dictionary<string, object>> tokensContext) {
            var tokens = tokensContext();

            // load corresponding events, as on one Rule several events of the same type could be configured 
            // with different parameters
            var events = _eventRepository.Table
                .Where(x => x.Category == category && x.Type == type && x.RuleRecord.Enabled)
                .ToList();

            var eventDescriptors = DescribeEvents().SelectMany(x => x.Descriptors);

            // take the first event which has a valid condition
            if (!events.Any(e => {
                    var eventCategory = e.Category;
                    var eventType = e.Type;

                    // look for the specified Event target/type
                    var descriptor = eventDescriptors
                        .Where(x => eventCategory == x.Category && eventType == x.Type)
                        .FirstOrDefault();

                    if (descriptor == null) {
                        return false;
                    }

                    var properties = FormParametersHelper.FromString(e.Parameters);
                    var context = new EventContext { Tokens = tokens, Properties = properties };

                    // check the condition
                    return descriptor.Condition(context);

                })) {

                return;
            }

            // load rules too for eager loading
            var rules = _ruleRepository.Table
                .Where(x => x.Enabled && x.Events.Any(e => e.Category == category && e.Type == type));

            // evaluate their conditions
            foreach (var e in events) {
                var rule = e.RuleRecord;

                ExecuteActions(rule.Actions, tokens);
            }
        }

        public void ExecuteActions(IEnumerable<ActionRecord> actions, Dictionary<string, object> tokens) {
            var actionDescriptors = DescribeActions().SelectMany(x => x.Descriptors);

            // execute each action associated with this rule
            foreach (var actionRecord in actions) {
                var actionCategory = actionRecord.Category;
                var actionType = actionRecord.Type;

                // look for the specified Event target/type
                var descriptor = actionDescriptors
                    .Where(x => actionCategory == x.Category && actionType == x.Type)
                    .FirstOrDefault();

                if (descriptor == null) {
                    continue;
                }

                // evaluate the tokens
                var parameters = _tokenizer.Replace(actionRecord.Parameters, tokens);

                var properties = FormParametersHelper.FromString(parameters);
                var context = new ActionContext { Properties = properties, Tokens = tokens };

                // execute the action
                var continuation = descriptor.Action(context);

                // early termination of the actions ?
                if (!continuation) {
                    break;
                }
            }
        }
    }
}