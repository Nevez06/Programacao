import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/features/feed/data/feed_repository.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/create_post_model.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/update_post_model.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_text_field.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class CreatePostPage extends StatefulWidget {
  const CreatePostPage({
    this.initialPost,
    super.key,
  });

  final PostModel? initialPost;

  bool get isEditing => initialPost != null;

  @override
  State<CreatePostPage> createState() => _CreatePostPageState();
}

class _CreatePostPageState extends State<CreatePostPage> {
  final _formKey = GlobalKey<FormState>();
  final _tituloController = TextEditingController();
  final _conteudoController = TextEditingController();
  final _imagemController = TextEditingController();
  final _categoriaController = TextEditingController();
  final _tipoConteudoController = TextEditingController();
  final _localizacaoController = TextEditingController();
  final _eventoIdController = TextEditingController();
  bool _commentsEnabled = true;
  bool _saving = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    final initial = widget.initialPost;
    if (initial != null) {
      _tituloController.text = initial.titulo ?? '';
      _conteudoController.text = initial.conteudo;
      _imagemController.text = initial.imagemUrl ?? '';
      _categoriaController.text = initial.categoria ?? '';
      _tipoConteudoController.text = initial.tipoConteudo ?? '';
      _localizacaoController.text = initial.localizacao ?? '';
      _eventoIdController.text =
          initial.eventoId != null ? '${initial.eventoId}' : '';
      _commentsEnabled = initial.commentsEnabled;
    }
  }

  @override
  void dispose() {
    _tituloController.dispose();
    _conteudoController.dispose();
    _imagemController.dispose();
    _categoriaController.dispose();
    _tipoConteudoController.dispose();
    _localizacaoController.dispose();
    _eventoIdController.dispose();
    super.dispose();
  }

  int? _readEventoId() {
    final raw = _eventoIdController.text.trim();
    if (raw.isEmpty) {
      return null;
    }
    return int.tryParse(raw);
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final eventoId = _readEventoId();
    if (_eventoIdController.text.trim().isNotEmpty && eventoId == null) {
      setState(() {
        _error = 'EventoId deve ser numerico.';
      });
      return;
    }

    setState(() {
      _saving = true;
      _error = null;
    });

    final repository = context.read<FeedRepository>();
    try {
      PostModel saved;
      if (widget.isEditing) {
        saved = await repository.updatePost(
          widget.initialPost!.id,
          UpdatePostModel(
            titulo: _tituloController.text.trim().isEmpty
                ? null
                : _tituloController.text.trim(),
            conteudo: _conteudoController.text.trim(),
            imagemUrl: _imagemController.text.trim().isEmpty
                ? null
                : _imagemController.text.trim(),
            categoria: _categoriaController.text.trim().isEmpty
                ? null
                : _categoriaController.text.trim(),
            tipoConteudo: _tipoConteudoController.text.trim().isEmpty
                ? null
                : _tipoConteudoController.text.trim(),
            localizacao: _localizacaoController.text.trim().isEmpty
                ? null
                : _localizacaoController.text.trim(),
            eventoId: eventoId,
            commentsEnabled: _commentsEnabled,
          ),
        );
      } else {
        saved = await repository.createPost(
          CreatePostModel(
            titulo: _tituloController.text.trim().isEmpty
                ? null
                : _tituloController.text.trim(),
            conteudo: _conteudoController.text.trim(),
            imagemUrl: _imagemController.text.trim().isEmpty
                ? null
                : _imagemController.text.trim(),
            categoria: _categoriaController.text.trim().isEmpty
                ? null
                : _categoriaController.text.trim(),
            tipoConteudo: _tipoConteudoController.text.trim().isEmpty
                ? null
                : _tipoConteudoController.text.trim(),
            localizacao: _localizacaoController.text.trim().isEmpty
                ? null
                : _localizacaoController.text.trim(),
            eventoId: eventoId,
            commentsEnabled: _commentsEnabled,
          ),
        );
      }

      if (!mounted) {
        return;
      }
      Navigator.of(context).pop(saved);
    } on ApiException catch (error) {
      setState(() {
        _error = error.message;
      });
    } catch (_) {
      setState(() {
        _error = 'Falha ao salvar publicacao.';
      });
    } finally {
      if (mounted) {
        setState(() {
          _saving = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final isEditing = widget.isEditing;
    return Scaffold(
      appBar: AppBar(
        title: Text(isEditing ? 'Editar post' : 'Criar post'),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                SectionTitle(
                  title: isEditing ? 'Editar publicacao' : 'Nova publicacao',
                  subtitle: isEditing
                      ? 'Atualiza via /api/posts/{id}'
                      : 'Cria via /api/posts',
                ),
                const SizedBox(height: 16),
                AppTextField(
                  controller: _tituloController,
                  label: 'Titulo (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _conteudoController,
                  label: 'Conteudo',
                  maxLines: 4,
                  validator: (value) {
                    if ((value ?? '').trim().isEmpty) {
                      return 'Informe o conteudo.';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _imagemController,
                  label: 'Imagem URL (opcional)',
                  hint: 'https://...',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _categoriaController,
                  label: 'Categoria (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _tipoConteudoController,
                  label: 'Tipo de conteudo (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _localizacaoController,
                  label: 'Localizacao (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _eventoIdController,
                  keyboardType: TextInputType.number,
                  label: 'EventoId (opcional)',
                ),
                const SizedBox(height: 12),
                SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Permitir comentarios'),
                  value: _commentsEnabled,
                  onChanged: (value) {
                    setState(() {
                      _commentsEnabled = value;
                    });
                  },
                ),
                if (_error != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    _error!,
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.error,
                    ),
                  ),
                ],
                const SizedBox(height: 16),
                PrimaryButton(
                  label: isEditing ? 'Salvar alteracoes' : 'Publicar',
                  icon: isEditing ? Icons.save : Icons.publish,
                  isLoading: _saving,
                  onPressed: _saving ? null : _save,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
